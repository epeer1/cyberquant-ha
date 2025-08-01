using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FSScore.WebApi.DataAccess;
using FSScore.WebApi.Models;

namespace FSScore.WebApi.Services
{
    /// <summary>
    /// Service implementation for generating reports using CalculateScorePerSnapshot stored procedure
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly DatabaseConnection _databaseConnection;

        public ReportService(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        }

        /// <summary>
        /// Generate Student Report for a specific snapshot
        /// </summary>
        public async Task<ApiResponse<StudentReport>> GetStudentReportAsync(int snapshotId)
        {
            try
            {
                if (snapshotId < 0)
                {
                    return ApiResponse<StudentReport>.ErrorResult("SnapshotId must be non-negative");
                }

                // Get zone scores using our stored procedure
                var zoneScores = await GetZoneScoresForSnapshotAsync(snapshotId);
                
                if (!zoneScores.Any())
                {
                    return ApiResponse<StudentReport>.ErrorResult($"No data found for snapshot {snapshotId}");
                }

                // Create the student report
                var report = new StudentReport
                {
                    SnapshotId = snapshotId,
                    CreationDate = DateTime.Now
                };

                // Get zones with valid scores only for top/bottom calculations
                var zonesWithScores = zoneScores.Where(z => z.Score.HasValue).ToList();

                if (zonesWithScores.Any())
                {
                    // Top 3 zones (highest scores)
                    report.TopZones = zonesWithScores
                        .OrderByDescending(z => z.Score.Value)
                        .Take(3)
                        .ToList();

                    // Bottom 3 zones (lowest scores)
                    report.BottomZones = zonesWithScores
                        .OrderBy(z => z.Score.Value)
                        .Take(3)
                        .ToList();

                    // Zones with score < 60
                    report.LowScoreZones = zonesWithScores
                        .Where(z => z.Score.Value < 60)
                        .OrderBy(z => z.Score.Value)
                        .ToList();
                }

                var message = $"Student report generated for snapshot {snapshotId} with {zoneScores.Count} zones analyzed";
                return ApiResponse<StudentReport>.SuccessResult(report, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<StudentReport>.ErrorResult($"Error generating student report: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate Principal Report across multiple snapshots
        /// </summary>
        public async Task<ApiResponse<PrincipalReport>> GetPrincipalReportAsync(List<int> snapshotIds)
        {
            try
            {
                if (snapshotIds == null || !snapshotIds.Any())
                {
                    return ApiResponse<PrincipalReport>.ErrorResult("At least one snapshot ID is required");
                }

                if (snapshotIds.Any(id => id < 0))
                {
                    return ApiResponse<PrincipalReport>.ErrorResult("All snapshot IDs must be non-negative");
                }

                // Get zone scores for all snapshots
                var allZoneScores = new List<ZoneScore>();
                var validSnapshots = new List<int>();

                foreach (var snapshotId in snapshotIds)
                {
                    var zoneScores = await GetZoneScoresForSnapshotAsync(snapshotId);
                    if (zoneScores.Any())
                    {
                        allZoneScores.AddRange(zoneScores);
                        validSnapshots.Add(snapshotId);
                    }
                }

                if (!allZoneScores.Any())
                {
                    return ApiResponse<PrincipalReport>.ErrorResult("No data found for any of the provided snapshots");
                }

                // Group by zone and calculate average scores
                var zoneAverages = allZoneScores
                    .Where(z => z.Score.HasValue)
                    .GroupBy(z => new { z.ZoneId, z.ZoneName })
                    .Select(g => new ZoneScore
                    {
                        ZoneId = g.Key.ZoneId,
                        ZoneName = g.Key.ZoneName,
                        Score = Math.Round(g.Average(z => z.Score.Value), 2),
                        TotalQuestions = g.Sum(z => z.TotalQuestions),
                        AnsweredQuestions = g.Sum(z => z.AnsweredQuestions)
                    })
                    .ToList();

                if (!zoneAverages.Any())
                {
                    return ApiResponse<PrincipalReport>.ErrorResult("No zones with valid scores found across snapshots");
                }

                // Find zone with lowest average score
                var lowestAverageZone = zoneAverages.OrderBy(z => z.Score.Value).First();

                var report = new PrincipalReport
                {
                    LowestAverageZone = lowestAverageZone,
                    AnalyzedSnapshots = validSnapshots,
                    CreationDate = DateTime.Now
                };

                var message = $"Principal report generated across {validSnapshots.Count} snapshots. Lowest average zone: {lowestAverageZone.ZoneName} ({lowestAverageZone.Score:F2})";
                return ApiResponse<PrincipalReport>.SuccessResult(report, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<PrincipalReport>.ErrorResult($"Error generating principal report: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to get zone scores for a specific snapshot using our stored procedure
        /// </summary>
        private async Task<List<ZoneScore>> GetZoneScoresForSnapshotAsync(int snapshotId)
        {
            using (var connection = _databaseConnection.CreateConnection())
            {
                // Call our CalculateScorePerSnapshot stored procedure
                var subjectScores = await connection.QueryAsync<dynamic>(
                    "CalculateScorePerSnapshot",
                    new { SnapshotId = snapshotId },
                    commandType: CommandType.StoredProcedure
                );

                // We need to get zone-level scores, so let's query zones directly
                // since the stored procedure returns subject-level aggregation
                var zoneScores = await connection.QueryAsync<ZoneScore>(@"
                    SELECT DISTINCT
                        z.ZoneId,
                        z.ZoneName,
                        CASE 
                            WHEN COUNT(q.QuestionId) > 0 THEN 
                                CAST(AVG(CAST(q.Score AS FLOAT)) AS DECIMAL(5,2))
                            ELSE NULL 
                        END as Score,
                        COUNT(q.QuestionId) as TotalQuestions,
                        COUNT(CASE WHEN q.Score IS NOT NULL THEN 1 END) as AnsweredQuestions
                    FROM Zones z
                    LEFT JOIN ZonesQuestions zq ON z.SnapshotId = zq.SnapshotId AND z.ZoneId = zq.ZoneId
                    LEFT JOIN Questions q ON zq.SnapshotId = q.SnapshotId AND zq.QuestionId = q.QuestionId
                    WHERE z.SnapshotId = @SnapshotId 
                      AND z.IsRelevant = 1
                      AND (q.QuestionId IS NULL OR q.IsRelevant = 1)
                    GROUP BY z.ZoneId, z.ZoneName
                    ORDER BY z.ZoneName",
                    new { SnapshotId = snapshotId }
                );

                return zoneScores.ToList();
            }
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using FSScore.WebApi.Models;

namespace FSScore.WebApi.Services
{
    /// <summary>
    /// Service interface for generating reports (Assignment Parts 4 & 5)
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generate Student Report for a specific snapshot
        /// Shows top 3 zones, bottom 3 zones, and zones with score < 60
        /// </summary>
        /// <param name="snapshotId">The snapshot ID to analyze</param>
        /// <returns>Student report with zone analysis</returns>
        Task<ApiResponse<StudentReport>> GetStudentReportAsync(int snapshotId);

        /// <summary>
        /// Generate Principal Report across multiple snapshots
        /// Finds the zone with the lowest average score across all provided snapshots
        /// </summary>
        /// <param name="snapshotIds">List of snapshot IDs to analyze</param>
        /// <returns>Principal report with lowest average zone</returns>
        Task<ApiResponse<PrincipalReport>> GetPrincipalReportAsync(List<int> snapshotIds);
    }
}
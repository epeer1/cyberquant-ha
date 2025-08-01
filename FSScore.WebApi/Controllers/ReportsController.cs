using System;
using System.Threading.Tasks;
using System.Web.Http;
using FSScore.WebApi.Models;
using FSScore.WebApi.Services;
using FSScore.WebApi.DataAccess;

namespace FSScore.WebApi.Controllers
{
    /// <summary>
    /// Reports API Controller (Assignment Parts 4 & 5)
    /// Provides Student Report and Principal Report endpoints
    /// </summary>
    [RoutePrefix("api/reports")]
    public class ReportsController : ApiController
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        }

        // Parameterless constructor for .NET Framework 4.8 Web API dependency resolution fallback
        public ReportsController() : this(CreateReportService())
        {
        }

        /// <summary>
        /// GET api/reports/student/{snapshotId}
        /// Generate Student Report showing top/bottom zones and zones with scores < 60
        /// Assignment Part 4
        /// </summary>
        /// <param name="snapshotId">The snapshot ID to analyze</param>
        /// <returns>Student report with zone analysis</returns>
        [HttpGet]
        [Route("student/{snapshotId:int}")]
        public async Task<IHttpActionResult> GetStudentReport(int snapshotId)
        {
            var result = await _reportService.GetStudentReportAsync(snapshotId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            if (result.Message.Contains("not found") || result.Message.Contains("No data"))
            {
                return NotFound();
            }
            
            return BadRequest(result.Message);
        }

        /// <summary>
        /// POST api/reports/principal
        /// Generate Principal Report showing zone with lowest average score across multiple snapshots
        /// Assignment Part 5
        /// </summary>
        /// <param name="request">Request containing list of snapshot IDs to analyze</param>
        /// <returns>Principal report with lowest average zone</returns>
        [HttpPost]
        [Route("principal")]
        public async Task<IHttpActionResult> GetPrincipalReport([FromBody] PrincipalReportRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (request.SnapshotIds == null || request.SnapshotIds.Count == 0)
            {
                return BadRequest("At least one snapshot ID is required");
            }

            var result = await _reportService.GetPrincipalReportAsync(request.SnapshotIds);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            if (result.Message.Contains("not found") || result.Message.Contains("No data"))
            {
                return NotFound();
            }
            
            return BadRequest(result.Message);
        }

        /// <summary>
        /// Factory method to create ReportService with dependencies for fallback constructor
        /// </summary>
        private static IReportService CreateReportService()
        {
            var dbConnection = new DatabaseConnection();
            return new ReportService(dbConnection);
        }
    }
}
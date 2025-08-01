using System.Collections.Generic;

namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Request model for Principal Report endpoint
    /// </summary>
    public class PrincipalReportRequest
    {
        /// <summary>
        /// List of snapshot IDs to analyze for lowest average zone score
        /// </summary>
        public List<int> SnapshotIds { get; set; } = new List<int>();
    }
}
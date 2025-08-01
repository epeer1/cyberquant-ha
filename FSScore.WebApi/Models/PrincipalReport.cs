using System;
using System.Collections.Generic;

namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Principal Report response model for Assignment Part 5
    /// </summary>
    public class PrincipalReport
    {
        public string Title { get; set; } = "Principal Report";
        public DateTime CreationDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Zone with the lowest average score across all provided snapshots
        /// </summary>
        public ZoneScore LowestAverageZone { get; set; }
        
        /// <summary>
        /// List of snapshot IDs that were analyzed
        /// </summary>
        public List<int> AnalyzedSnapshots { get; set; } = new List<int>();
    }
}
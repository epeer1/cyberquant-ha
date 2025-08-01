using System;
using System.Collections.Generic;

namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Student Report response model for Assignment Part 4
    /// </summary>
    public class StudentReport
    {
        public string Title { get; set; } = "Student report";
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public int SnapshotId { get; set; }
        
        /// <summary>
        /// Top 3 zones with highest scores
        /// </summary>
        public List<ZoneScore> TopZones { get; set; } = new List<ZoneScore>();
        
        /// <summary>
        /// Bottom 3 zones with lowest scores
        /// </summary>
        public List<ZoneScore> BottomZones { get; set; } = new List<ZoneScore>();
        
        /// <summary>
        /// All zones with score less than 60
        /// </summary>
        public List<ZoneScore> LowScoreZones { get; set; } = new List<ZoneScore>();
    }
}
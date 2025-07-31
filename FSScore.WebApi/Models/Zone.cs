namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Represents a zone entity from the database
    /// </summary>
    public class Zone
    {
        public int SnapshotId { get; set; }
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
        public bool IsRelevant { get; set; }
    }
}
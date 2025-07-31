namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Represents a subject entity from the database
    /// </summary>
    public class Subject
    {
        public int SnapshotId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
    }
}
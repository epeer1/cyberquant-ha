namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Represents a zone with its calculated score
    /// </summary>
    public class ZoneScore
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
        public decimal? Score { get; set; }  // Nullable - zones might not have scores
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
    }
}
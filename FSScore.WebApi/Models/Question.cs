using System;

namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Represents a question entity from the database
    /// </summary>
    public class Question
    {
        public int SnapshotId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int? Score { get; set; }  // Nullable - NULL means unanswered
        public bool IsRelevant { get; set; }
        public int TestId { get; set; }
    }
}
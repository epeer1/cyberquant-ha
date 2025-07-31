namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Request model for creating a new question
    /// </summary>
    public class CreateQuestionRequest
    {
        public int SnapshotId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int? Score { get; set; }  // Nullable - NULL means unanswered
        public bool IsRelevant { get; set; }
        public int TestId { get; set; }

        /// <summary>
        /// Convert request to Question entity
        /// </summary>
        public Question ToQuestion()
        {
            return new Question
            {
                SnapshotId = this.SnapshotId,
                QuestionId = this.QuestionId,
                QuestionText = this.QuestionText,
                Score = this.Score,
                IsRelevant = this.IsRelevant,
                TestId = this.TestId
            };
        }
    }
}
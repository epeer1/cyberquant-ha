namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Request model for updating an existing question
    /// Assignment requirement: Update question - name, IsRelevant and score values (score can't be negative)
    /// </summary>
    public class UpdateQuestionRequest
    {
        public string QuestionText { get; set; }  // name
        public bool IsRelevant { get; set; }      // IsRelevant
        public int? Score { get; set; }           // score (can't be negative, nullable for unanswered)

        /// <summary>
        /// Convert request to Question entity for update
        /// </summary>
        public Question ToQuestion()
        {
            return new Question
            {
                QuestionText = this.QuestionText,
                IsRelevant = this.IsRelevant,
                Score = this.Score
                // SnapshotId, QuestionId, and TestId will be set by the service
            };
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using FSScore.WebApi.Models;

namespace FSScore.WebApi.DataAccess
{
    /// <summary>
    /// Repository interface for Question data access operations
    /// </summary>
    public interface IQuestionRepository
    {
        /// <summary>
        /// Get all questions for a specific snapshot
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <returns>List of questions in the snapshot</returns>
        Task<IEnumerable<Question>> GetQuestionsBySnapshotAsync(int snapshotId);

        /// <summary>
        /// Get a specific question by snapshot and question ID
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <returns>Question if found, null otherwise</returns>
        Task<Question> GetQuestionAsync(int snapshotId, int questionId);

        /// <summary>
        /// Create a new question in a specific snapshot
        /// </summary>
        /// <param name="question">Question to create</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> CreateQuestionAsync(Question question);

        /// <summary>
        /// Update an existing question
        /// </summary>
        /// <param name="question">Question with updated values</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateQuestionAsync(Question question);

        /// <summary>
        /// Delete a question from a specific snapshot
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteQuestionAsync(int snapshotId, int questionId);

        /// <summary>
        /// Check if a question exists in a specific snapshot
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> QuestionExistsAsync(int snapshotId, int questionId);
    }
}
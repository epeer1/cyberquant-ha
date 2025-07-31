using System.Collections.Generic;
using System.Threading.Tasks;
using FSScore.WebApi.Models;

namespace FSScore.WebApi.Services
{
    /// <summary>
    /// Service interface for Question business logic operations
    /// </summary>
    public interface IQuestionService
    {
        /// <summary>
        /// Get all questions for a specific snapshot
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <returns>API response with list of questions</returns>
        Task<ApiResponse<IEnumerable<Question>>> GetQuestionsBySnapshotAsync(int snapshotId);

        /// <summary>
        /// Get a specific question by snapshot and question ID
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <returns>API response with question data</returns>
        Task<ApiResponse<Question>> GetQuestionAsync(int snapshotId, int questionId);

        /// <summary>
        /// Create a new question in a specific snapshot
        /// </summary>
        /// <param name="question">Question to create</param>
        /// <returns>API response with creation result</returns>
        Task<ApiResponse<Question>> CreateQuestionAsync(Question question);

        /// <summary>
        /// Update an existing question (name, IsRelevant, score)
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <param name="updatedQuestion">Question with updated values</param>
        /// <returns>API response with update result</returns>
        Task<ApiResponse<Question>> UpdateQuestionAsync(int snapshotId, int questionId, Question updatedQuestion);

        /// <summary>
        /// Delete a question from a specific snapshot
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <returns>API response with deletion result</returns>
        Task<ApiResponse<bool>> DeleteQuestionAsync(int snapshotId, int questionId);
    }
}
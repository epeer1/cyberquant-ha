using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSScore.WebApi.DataAccess;
using FSScore.WebApi.Models;

namespace FSScore.WebApi.Services
{
    /// <summary>
    /// Service implementation for Question business logic with validation
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionService(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository ?? throw new ArgumentNullException(nameof(questionRepository));
        }

        /// <summary>
        /// Get all questions for a specific snapshot
        /// </summary>
        public async Task<ApiResponse<IEnumerable<Question>>> GetQuestionsBySnapshotAsync(int snapshotId)
        {
            try
            {
                if (snapshotId < 0)
                {
                    return ApiResponse<IEnumerable<Question>>.ErrorResult("SnapshotId must be non-negative");
                }

                var questions = await _questionRepository.GetQuestionsBySnapshotAsync(snapshotId);
                var questionsList = questions.ToList();

                return ApiResponse<IEnumerable<Question>>.SuccessResult(
                    questionsList, 
                    $"Retrieved {questionsList.Count} questions for snapshot {snapshotId}"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<Question>>.ErrorResult($"Error retrieving questions: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific question by snapshot and question ID
        /// </summary>
        public async Task<ApiResponse<Question>> GetQuestionAsync(int snapshotId, int questionId)
        {
            try
            {
                if (snapshotId < 0)
                {
                    return ApiResponse<Question>.ErrorResult("SnapshotId must be non-negative");
                }

                if (questionId <= 0)
                {
                    return ApiResponse<Question>.ErrorResult("QuestionId must be positive");
                }

                var question = await _questionRepository.GetQuestionAsync(snapshotId, questionId);
                
                if (question == null)
                {
                    return ApiResponse<Question>.ErrorResult(
                        $"Question {questionId} not found in snapshot {snapshotId}"
                    );
                }

                return ApiResponse<Question>.SuccessResult(question, "Question retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<Question>.ErrorResult($"Error retrieving question: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new question in a specific snapshot
        /// </summary>
        public async Task<ApiResponse<Question>> CreateQuestionAsync(Question question)
        {
            try
            {
                // Validate input
                var validationResult = ValidateQuestion(question, isUpdate: false);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Check if question already exists
                var exists = await _questionRepository.QuestionExistsAsync(question.SnapshotId, question.QuestionId);
                if (exists)
                {
                    return ApiResponse<Question>.ErrorResult(
                        $"Question {question.QuestionId} already exists in snapshot {question.SnapshotId}"
                    );
                }

                // Create question
                var success = await _questionRepository.CreateQuestionAsync(question);
                if (!success)
                {
                    return ApiResponse<Question>.ErrorResult("Failed to create question");
                }

                return ApiResponse<Question>.SuccessResult(question, "Question created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<Question>.ErrorResult($"Error creating question: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing question (name, IsRelevant, score)
        /// </summary>
        public async Task<ApiResponse<Question>> UpdateQuestionAsync(int snapshotId, int questionId, Question updatedQuestion)
        {
            try
            {
                // Validate IDs
                if (snapshotId < 0)
                {
                    return ApiResponse<Question>.ErrorResult("SnapshotId must be non-negative");
                }

                if (questionId <= 0)
                {
                    return ApiResponse<Question>.ErrorResult("QuestionId must be positive");
                }

                // Ensure the question exists
                var existingQuestion = await _questionRepository.GetQuestionAsync(snapshotId, questionId);
                if (existingQuestion == null)
                {
                    return ApiResponse<Question>.ErrorResult(
                        $"Question {questionId} not found in snapshot {snapshotId}"
                    );
                }

                // Set the correct IDs and preserve TestId
                updatedQuestion.SnapshotId = snapshotId;
                updatedQuestion.QuestionId = questionId;
                updatedQuestion.TestId = existingQuestion.TestId; // Preserve original TestId

                // Validate the updated question
                var validationResult = ValidateQuestion(updatedQuestion, isUpdate: true);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Update question
                var success = await _questionRepository.UpdateQuestionAsync(updatedQuestion);
                if (!success)
                {
                    return ApiResponse<Question>.ErrorResult("Failed to update question");
                }

                return ApiResponse<Question>.SuccessResult(updatedQuestion, "Question updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<Question>.ErrorResult($"Error updating question: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a question from a specific snapshot
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteQuestionAsync(int snapshotId, int questionId)
        {
            try
            {
                if (snapshotId < 0)
                {
                    return ApiResponse<bool>.ErrorResult("SnapshotId must be non-negative");
                }

                if (questionId <= 0)
                {
                    return ApiResponse<bool>.ErrorResult("QuestionId must be positive");
                }

                // Check if question exists
                var exists = await _questionRepository.QuestionExistsAsync(snapshotId, questionId);
                if (!exists)
                {
                    return ApiResponse<bool>.ErrorResult(
                        $"Question {questionId} not found in snapshot {snapshotId}"
                    );
                }

                // Delete question
                var success = await _questionRepository.DeleteQuestionAsync(snapshotId, questionId);
                if (!success)
                {
                    return ApiResponse<bool>.ErrorResult("Failed to delete question");
                }

                return ApiResponse<bool>.SuccessResult(true, "Question deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Error deleting question: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate question data according to business rules
        /// </summary>
        private ApiResponse<Question> ValidateQuestion(Question question, bool isUpdate)
        {
            if (question == null)
            {
                return ApiResponse<Question>.ErrorResult("Question cannot be null");
            }

            if (string.IsNullOrWhiteSpace(question.QuestionText))
            {
                return ApiResponse<Question>.ErrorResult("Question text is required");
            }

            if (question.QuestionText.Length > 1000) // Reasonable limit
            {
                return ApiResponse<Question>.ErrorResult("Question text cannot exceed 1000 characters");
            }

            if (question.SnapshotId < 0)
            {
                return ApiResponse<Question>.ErrorResult("SnapshotId must be non-negative");
            }

            if (question.QuestionId <= 0)
            {
                return ApiResponse<Question>.ErrorResult("QuestionId must be positive");
            }

            // Score validation - can be null (unanswered), but if provided must be non-negative
            if (question.Score.HasValue && question.Score.Value < 0)
            {
                return ApiResponse<Question>.ErrorResult("Score cannot be negative");
            }

            if (question.Score.HasValue && question.Score.Value > 100)
            {
                return ApiResponse<Question>.ErrorResult("Score cannot exceed 100");
            }

            if (!isUpdate && question.TestId <= 0)
            {
                return ApiResponse<Question>.ErrorResult("TestId must be positive for new questions");
            }

            return ApiResponse<Question>.SuccessResult(question, "Validation passed");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using FSScore.WebApi.Models;

namespace FSScore.WebApi.DataAccess
{
    /// <summary>
    /// Repository implementation for Question data access using Dapper
    /// </summary>
    public class QuestionRepository : IQuestionRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public QuestionRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        /// <summary>
        /// Get all questions for a specific snapshot
        /// </summary>
        public async Task<IEnumerable<Question>> GetQuestionsBySnapshotAsync(int snapshotId)
        {
            const string sql = @"
                SELECT SnapshotId, QuestionId, QuestionText, Score, IsRelevant, TestId
                FROM Questions 
                WHERE SnapshotId = @SnapshotId
                ORDER BY QuestionId";

            using (var connection = _dbConnection.CreateConnection())
            {
                return await connection.QueryAsync<Question>(sql, new { SnapshotId = snapshotId });
            }
        }

        /// <summary>
        /// Get a specific question by snapshot and question ID
        /// </summary>
        public async Task<Question> GetQuestionAsync(int snapshotId, int questionId)
        {
            const string sql = @"
                SELECT SnapshotId, QuestionId, QuestionText, Score, IsRelevant, TestId
                FROM Questions 
                WHERE SnapshotId = @SnapshotId AND QuestionId = @QuestionId";

            using (var connection = _dbConnection.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Question>(sql, 
                    new { SnapshotId = snapshotId, QuestionId = questionId });
            }
        }

        /// <summary>
        /// Create a new question in a specific snapshot
        /// </summary>
        public async Task<bool> CreateQuestionAsync(Question question)
        {
            const string sql = @"
                INSERT INTO Questions (SnapshotId, QuestionId, QuestionText, Score, IsRelevant, TestId)
                VALUES (@SnapshotId, @QuestionId, @QuestionText, @Score, @IsRelevant, @TestId)";

            using (var connection = _dbConnection.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(sql, question);
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Update an existing question
        /// </summary>
        public async Task<bool> UpdateQuestionAsync(Question question)
        {
            const string sql = @"
                UPDATE Questions 
                SET QuestionText = @QuestionText, 
                    Score = @Score, 
                    IsRelevant = @IsRelevant
                WHERE SnapshotId = @SnapshotId AND QuestionId = @QuestionId";

            using (var connection = _dbConnection.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(sql, question);
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Delete a question from a specific snapshot
        /// </summary>
        public async Task<bool> DeleteQuestionAsync(int snapshotId, int questionId)
        {
            const string sql = @"
                DELETE FROM Questions 
                WHERE SnapshotId = @SnapshotId AND QuestionId = @QuestionId";

            using (var connection = _dbConnection.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(sql, 
                    new { SnapshotId = snapshotId, QuestionId = questionId });
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Check if a question exists in a specific snapshot
        /// </summary>
        public async Task<bool> QuestionExistsAsync(int snapshotId, int questionId)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM Questions 
                WHERE SnapshotId = @SnapshotId AND QuestionId = @QuestionId";

            using (var connection = _dbConnection.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(sql, 
                    new { SnapshotId = snapshotId, QuestionId = questionId });
                return count > 0;
            }
        }
    }
}
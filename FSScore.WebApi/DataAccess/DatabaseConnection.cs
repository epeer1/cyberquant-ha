using System;
using System.Configuration;
using System.Data.SqlClient;

namespace FSScore.WebApi.DataAccess
{
    /// <summary>
    /// Manages database connections for the application
    /// </summary>
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["GradesDB"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("GradesDB connection string not found in Web.config");
            }
        }

        /// <summary>
        /// Creates a new SQL connection
        /// </summary>
        /// <returns>SQL connection ready to open</returns>
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        public bool TestConnection()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
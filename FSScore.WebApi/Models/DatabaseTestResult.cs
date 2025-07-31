namespace FSScore.WebApi.Models
{
    /// <summary>
    /// Model for database connection test results
    /// </summary>
    public class DatabaseTestResult
    {
        public string DatabaseStatus { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
    }
}
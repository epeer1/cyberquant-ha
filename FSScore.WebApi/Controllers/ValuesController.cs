using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FSScore.WebApi.DataAccess;
using FSScore.WebApi.Models;

namespace FSScore.WebApi.Controllers
{
	public class ValuesController : ApiController
	{
		// GET api/values
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/values/test-db
		[HttpGet]
		[Route("api/values/test-db")]
		public ApiResponse<DatabaseTestResult> TestDatabase()
		{
			try
			{
				var dbConnection = new DatabaseConnection();
				bool isConnected = dbConnection.TestConnection();

				if (isConnected)
				{
					return ApiResponse<DatabaseTestResult>.SuccessResult(
						new DatabaseTestResult 
						{ 
							DatabaseStatus = "Connected", 
							Server = "localhost\\SQLEXPRESS", 
							Database = "Grades" 
						},
						"Database connection successful"
					);
				}
				else
				{
					return ApiResponse<DatabaseTestResult>.ErrorResult(
						"Database connection failed. Please check connection string and ensure SQL Server is running."
					);
				}
			}
			catch (Exception ex)
			{
				return ApiResponse<DatabaseTestResult>.ErrorResult($"Database connection error: {ex.Message}");
			}
		}

		// GET api/values/5
		public string Get(int id)
		{
			return "value";
		}

		// POST api/values
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		public void Delete(int id)
		{
		}
	}
}

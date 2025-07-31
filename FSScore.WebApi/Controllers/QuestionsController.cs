using System.Threading.Tasks;
using System.Web.Http;
using FSScore.WebApi.Models;
using FSScore.WebApi.Services;

namespace FSScore.WebApi.Controllers
{
    /// <summary>
    /// RESTful API controller for Questions CRUD operations
    /// Assignment Part 3: Basic CRUD ability against questions table for specific snapshot
    /// </summary>
    [RoutePrefix("api/questions")]
    public class QuestionsController : ApiController
    {
        private readonly IQuestionService _questionService;

        // Default constructor for Web API framework
        public QuestionsController() : this(CreateQuestionService())
        {
        }

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        // Factory method to create dependencies manually
        private static IQuestionService CreateQuestionService()
        {
            var dbConnection = new DataAccess.DatabaseConnection();
            var repository = new DataAccess.QuestionRepository(dbConnection);
            return new Services.QuestionService(repository);
        }

        /// <summary>
        /// GET api/questions/snapshot/{snapshotId}
        /// Get all questions for a specific snapshot
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <returns>List of questions in the snapshot</returns>
        [HttpGet]
        [Route("snapshot/{snapshotId:int}")]
        public async Task<IHttpActionResult> GetQuestionsBySnapshot(int snapshotId)
        {
            var result = await _questionService.GetQuestionsBySnapshotAsync(snapshotId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result.Message);
        }

        /// <summary>
        /// GET api/questions/snapshot/{snapshotId}/question/{questionId}
        /// Get a specific question by snapshot and question ID
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <returns>Question details</returns>
        [HttpGet]
        [Route("snapshot/{snapshotId:int}/question/{questionId:int}")]
        public async Task<IHttpActionResult> GetQuestion(int snapshotId, int questionId)
        {
            var result = await _questionService.GetQuestionAsync(snapshotId, questionId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            if (result.Message.Contains("not found"))
            {
                return NotFound();
            }
            
            return BadRequest(result.Message);
        }

        /// <summary>
        /// POST api/questions
        /// Create a new question for a snapshot
        /// </summary>
        /// <param name="request">Question data to create</param>
        /// <returns>Created question details</returns>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Question data is required");
            }

            var question = request.ToQuestion();
            var result = await _questionService.CreateQuestionAsync(question);
            
            if (result.Success)
            {
                // Return 201 Created with location header
                var location = $"api/questions/snapshot/{question.SnapshotId}/question/{question.QuestionId}";
                return Created(location, result);
            }
            
            if (result.Message.Contains("already exists"))
            {
                return Conflict();
            }
            
            return BadRequest(result.Message);
        }

        /// <summary>
        /// PUT api/questions/snapshot/{snapshotId}/question/{questionId}
        /// Update question - name, IsRelevant and score values (score can't be negative)
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <param name="request">Updated question data</param>
        /// <returns>Updated question details</returns>
        [HttpPut]
        [Route("snapshot/{snapshotId:int}/question/{questionId:int}")]
        public async Task<IHttpActionResult> UpdateQuestion(int snapshotId, int questionId, [FromBody] UpdateQuestionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Question data is required");
            }

            var question = request.ToQuestion();
            var result = await _questionService.UpdateQuestionAsync(snapshotId, questionId, question);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            if (result.Message.Contains("not found"))
            {
                return NotFound();
            }
            
            return BadRequest(result.Message);
        }

        /// <summary>
        /// DELETE api/questions/snapshot/{snapshotId}/question/{questionId}
        /// Remove a question by snapshot
        /// </summary>
        /// <param name="snapshotId">The snapshot ID</param>
        /// <param name="questionId">The question ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete]
        [Route("snapshot/{snapshotId:int}/question/{questionId:int}")]
        public async Task<IHttpActionResult> DeleteQuestion(int snapshotId, int questionId)
        {
            var result = await _questionService.DeleteQuestionAsync(snapshotId, questionId);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            if (result.Message.Contains("not found"))
            {
                return NotFound();
            }
            
            return BadRequest(result.Message);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Models.Entities;

namespace Selfaware.Features.Quizzes
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpPost("create-quiz")]
        public async Task<ActionResult<CustomResponse<Guid>>> createQuiz([FromBody] CreateQuizDto model)
        {

            Guid newQuizId = await _quizService.CreateQuizAsync(model);

            return Ok(new CustomResponse<Guid>
            {
                Success = true,
                Message = "Quiz created successfully",
                Data = newQuizId
            });
        }

    }
}

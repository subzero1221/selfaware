using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Quizzes
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [Authorize(Roles = "Admin")]
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

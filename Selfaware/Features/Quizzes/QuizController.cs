using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Auth.Entities;
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
        public async Task<ActionResult> createQuiz([FromBody] CreateQuizDto dto)
        {

            var result = await _quizService.CreateQuizAsync(dto);

            return Ok(CustomResponse<QuizDto>.SuccessResponse( result.Data, result.Message ));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("bulk-create")]
        public async Task<ActionResult> bulkQuizUpload([FromForm] BulkQuizUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0) return BadRequest(CustomResponse<QuizUploadedResponseDto>.ErrorResponse("No file was uploaded"));
            using var stream = dto.File.OpenReadStream();
            var createdQuiz = await _quizService.BulkImportQuizAsync(dto.Title, dto.TimeLimitInMinutes, stream, dto.Description);
            if (!createdQuiz.Success)
                return BadRequest(CustomResponse<QuizUploadedResponseDto>.ErrorResponse(createdQuiz.Message ?? "Failed to extract questions."));

            return Ok(CustomResponse<QuizUploadedResponseDto>.SuccessResponse(createdQuiz.Data, "Quiz uploaded successfully"));
        }
         
    }
}

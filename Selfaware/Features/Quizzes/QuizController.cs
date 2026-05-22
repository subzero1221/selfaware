using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Shared.Models;
using System.Security.Claims;

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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (dto.File == null || dto.File.Length == 0) return BadRequest(CustomResponse<QuizSummaryDto>.ErrorResponse("No file was uploaded"));
            using var stream = dto.File.OpenReadStream();
            var createdQuiz = await _quizService.BulkImportQuizAsync(dto.Title, dto.TimeLimitInMinutes, stream, dto.Description, userId);
            if (!createdQuiz.Success)
                return BadRequest(CustomResponse<QuizSummaryDto>.ErrorResponse(createdQuiz.Message ?? "Failed to extract questions."));

            return Ok(CustomResponse<QuizSummaryDto>.SuccessResponse(createdQuiz.Data, createdQuiz.Message));
        }

        [Authorize(Roles="Admin")]
        [HttpGet]
        public async Task<ActionResult> GetMyQuizzesAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var result = await _quizService.GetMyQuizzesAsync(userId);

            if (result == null)
            {
                return NotFound(CustomResponse<string>.ErrorResponse("Quiz not found."));
            }

            return Ok(CustomResponse<GetMyQuizzesDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetMyQuizzesAsync([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

 
            var result = await _quizService.GetSingleQuizAsync(id, userId);

    
            if (result == null)
            {
                return NotFound(CustomResponse<string>.ErrorResponse("Quiz not found."));
            }

            return Ok(CustomResponse<QuizDetailsDto>.SuccessResponse(result.Data, result.Message));
        }
    } 
    }


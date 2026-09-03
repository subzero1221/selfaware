using System.Security.Claims;
using Google.GenAI.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.DTOs.Selfaware.Features.Quizzes.DTOs;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Quizzes
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IQuizGeneratorService _quizGeneratorService;
        private readonly IQuizEditorService _quizEditorService;

        public QuizController(
            IQuizService quizService,
            IQuizGeneratorService quizGeneratorService,
            IQuizEditorService quizEditorService
        )
        {
            _quizService = quizService;
            _quizGeneratorService = quizGeneratorService;
            _quizEditorService = quizEditorService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> createQuiz([FromBody] CreateQuizDto dto)
        {
            var result = await _quizService.CreateQuizAsync(dto);

            return Ok(CustomResponse<QuizDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> PutQuiz([FromRoute] Guid id, [FromBody] PutQuizDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(
                    CustomResponse<Guid>.ErrorResponse("Your have no permission for this action")
                );

            if (id != dto.Id)
            {
                return BadRequest(CustomResponse<Guid>.ErrorResponse("Mismatched Quiz ID"));
            }

            var result = await _quizService.PutQuizAsync(dto, id, userId);
            if (!result.Success)
            {
                return BadRequest(CustomResponse<Guid>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<Guid>.SuccessResponse(result.Data, result.Message));
        }

        /*[Authorize(Roles = "Admin")]
        [HttpPost("bulk-create")]
        public async Task<ActionResult> bulkQuizUpload([FromForm] BulkQuizUpload dto)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (dto.File == null || dto.File.Length == 0) return BadRequest(CustomResponse<QuizSummaryDto>.ErrorResponse("No file was uploaded"));
            using var stream = dto.File.OpenReadStream();
            var createdQuiz = await _quizService.BulkImportQuizAsync(dto.Title, dto.TimeLimitInMinutes, stream, dto.Description, userId);
            if (!createdQuiz.Success)
                return BadRequest(CustomResponse<QuizSummaryDto>.ErrorResponse(createdQuiz.Message ?? "Failed to extract questions."));

            return Ok(CustomResponse<QuizSummaryDto>.SuccessResponse(createdQuiz.Data, createdQuiz.Message));
        }*/

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> GetMyQuizzesAsync([FromRoute] QuizType? quizType )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _quizService.GetMyQuizzesAsync(userId, quizType);

            if (result == null)
            {
                return NotFound(CustomResponse<string>.ErrorResponse("Quiz not found."));
            }

            return Ok(CustomResponse<GetQuizzesDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ai-generate")]
        public async Task<ActionResult> ExtractExistingQuiz(
            [FromForm] ExtractQuizRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("You must be logged in to generate a quiz.");
            }
            var result = await _quizGeneratorService.ExtractExistingQuizAsync(
                dto,
                userId,
                cancellationToken
            );

            if (!result.Success)
            {
                return BadRequest(CustomResponse<Guid>.ErrorResponse(result.Message));
            }

            return Ok(
                CustomResponse<Guid>.SuccessResponse(result.Data, "Quiz generated Succesfully")
            );
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetMyQuizAsync([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _quizService.GetSingleQuizAsync(id, userId);

            if (result == null)
            {
                return NotFound(CustomResponse<string>.ErrorResponse("Quiz not found."));
            }

            return Ok(CustomResponse<QuizDetailsDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteQuiz([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _quizService.DeleteQuizAsync(id, userId);
            if (result == null)
            {
                return NotFound(CustomResponse<string>.ErrorResponse("Quiz not found."));
            }

            return Ok(CustomResponse<Guid>.SuccessResponse(result.Data, result.Message));
        }

        ///////
        /////////////
        //Edit Controllers
        /////////////
        ///////

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/settings")]
        public async Task<ActionResult> EditQuizSettings(
            [FromRoute] Guid id,
            [FromBody] EditQuizSettingsDto dto
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _quizEditorService.EditSettingsAsync(id, userId, dto);
            if (result == null)
            {
                return NotFound(CustomResponse<string>.ErrorResponse("Quiz not found."));
            }

            return Ok(CustomResponse<Guid>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/questions/{questionId:guid}")]
        public async Task<ActionResult> EditQuizQuestions(
            [FromRoute] Guid id,
            Guid questionId,
            [FromBody] EditQuestionDto dto
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _quizEditorService.EditQuestionAsync(id, questionId, userId, dto);
            if (result == null)
            {
                return NotFound(CustomResponse<string>.ErrorResponse("Question not found."));
            }

            return Ok(CustomResponse<Guid>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}/questions/{questionId:guid}")]
        public async Task<ActionResult> DeleteQuizQuestion([FromRoute] Guid id, Guid questionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _quizEditorService.DeleteQuestionAsync(id, questionId, userId);
            if (result == null)
            {
                return NotFound(CustomResponse<string>.ErrorResponse("Question not found."));
            }

            return Ok(CustomResponse<Guid>.SuccessResponse(result.Data, result.Message));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Features.Survey.SurveySession.Dtos;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Survey.SurveySession
{

    [ApiController]
    [Route("api/[controller]")]
    public class SurveySessionController:ControllerBase
    {
        public readonly ISurveySessionService _surveySessionService;        

        public SurveySessionController(ISurveySessionService surveySessionService)
        {
            _surveySessionService = surveySessionService;
        }

        [HttpPost]
        public async Task<ActionResult> StartSurveySession([FromBody] StartSurveySessionDto dto)
        {
            var result = await _surveySessionService.StartSurveySessionAsync(dto);
            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<SurveySessionDto>.SuccessResponse(result.Data, "Survey started successfuly"));
        }

        [HttpGet]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetSurveySession([FromRoute] Guid id)
        {
            var result = await _surveySessionService.GetSurveySessionAsync(id);
            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<SurveySessionDto>.SuccessResponse(result.Data, "Survey started successfuly"));
        }

        [HttpGet("{surveyId:guid}/question/{order:int}")]
        public async Task<ActionResult> GetQuestionForSurveySession([FromRoute] Guid surveyId, [FromRoute] int order)
        {
            var result = await _surveySessionService.GetQuestionAsync(surveyId, order);


            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<NextQuestionResponseDto>.SuccessResponse(result.Data, "Question fetched succesfully"));
        }

        [HttpPost("answer")]
        public async Task<ActionResult> SubmitAnswer([FromBody] SubmitSurveyAnswerDto dto)
        {
            var result = await _surveySessionService.SubmitAnswerAsync(dto);
            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }
            return Ok(CustomResponse<UserAnswerDto>.SuccessResponse(result.Data, result.Message));
        }

    }
}

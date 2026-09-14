using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Quizzes.DTOs;
using Selfaware.Features.Quizzes.Entities;
using Selfaware.Features.Survey.SurveySession.Dtos;
using Selfaware.Shared.Models;

namespace Selfaware.Features.Survey.SurveySession
{
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
            if (result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<SurveySessionDto>.SuccessResponse(result.Data, "Survey started successfuly"));
        }

        [HttpGet("question")]
        public async Task<ActionResult> GetQuestionForSurveySession([FromBody] GetQuestionForSurveySessionDto dto)
        {
            var result = await _surveySessionService.GetQuestionForSurveySessionAsync(dto);


            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<QuestionDto>.SuccessResponse(result.Data, "Question fetched succesfully"));
        }

    }
}

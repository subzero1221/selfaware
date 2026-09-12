using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Survey.DTOs;
using Selfaware.Shared.Models;
using System.Security.Claims;


namespace Selfaware.Features.Survey
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController:ControllerBase
    {

        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }


        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult> ActivateSurvey([FromBody] ActivateSurveyDto dto)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _surveyService.ActivateSurveyAsync(dto, userId);

            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<SurveyDto>.SuccessResponse(result.Data, "Survey activated successfully"));
        }
    
       public async Task<ActionResult> GetMyActiveSurveys()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _surveyService.GetMyActiveSurveysAsync(userId);
            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }
            return Ok(CustomResponse<List<SurveyDto>>.SuccessResponse(result.Data, "Survey activated successfully"));
        }
   
    }
}

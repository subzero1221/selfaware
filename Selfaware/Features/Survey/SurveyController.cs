using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Shared.Models;
using Selfaware.Features.Survey.DTOs;


namespace Selfaware.Features.Survey
{
    [ApiController]
    [Route("$/api[controller]")]
    public class SurveyController:ControllerBase
    {
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult> ActivateSurvey([FromBody] ActivateSurveyDto dto)
        {

            return Ok(CustomResponse<string>.SuccessResponse("nah"));
        }
    
   
   
    }
}

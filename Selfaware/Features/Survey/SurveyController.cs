using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Shared.Models;


namespace Selfaware.Features.Survey
{
    [ApiController]
    [Route("$/api[controller]")]
    public class SurveyController:ControllerBase
    {
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult> StartSurvey()
        {
            return Ok(CustomResponse<string>.SuccessResponse("nah"));
        }
    }
}

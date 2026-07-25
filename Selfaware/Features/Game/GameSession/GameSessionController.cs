using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Shared.Models;
using System.Security.Claims;

namespace Selfaware.Features.Game.GameSession
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameSessionController : ControllerBase
    {
        private readonly IGameSessionService _gameSessionService;

        public GameSessionController(IGameSessionService gameSessionService)
        {
            _gameSessionService = gameSessionService;
        }

        [HttpGet("{joinCode}/{playerId}")]
        public async Task<ActionResult> GetGame(
            [FromRoute] string joinCode,
            [FromRoute] string playerId
        )
        {
            var result = await _gameSessionService.GetGameAsync(joinCode, playerId);

            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<GameDto>.SuccessResponse(result.Data, result.Message));
        }

        [HttpGet("{joinCode}")]
        public async Task<ActionResult> GetGameForHost(
           [FromRoute] string joinCode
       )
        {
            var result = await _gameSessionService.GetGameForHostAsync(joinCode);

            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<GameDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{joinCode}")]
        public async Task<ActionResult> SaveFinishedGame([FromRoute] string joinCode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(
                    CustomResponse<Guid>.ErrorResponse("Your have no permission for this action")
                );
            var result = await _gameSessionService.SaveFinishedGameAsync(joinCode, userId);

            return Ok(CustomResponse<string>.SuccessResponse(result.Message));
        }           

    }
}

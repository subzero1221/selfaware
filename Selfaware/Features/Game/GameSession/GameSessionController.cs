using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Game.GameSession.DTOs;
using Selfaware.Shared.Models;


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
        public async Task<ActionResult> GetGame([FromRoute] string joinCode, [FromRoute] string playerId)
        {
            var result = await _gameSessionService.GetGameAsync(joinCode, playerId);

            if (!result.Success)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse(result.Message));
            }

            return Ok(CustomResponse<GameDto>.SuccessResponse(result.Data, result.Message));
        }
    }
}

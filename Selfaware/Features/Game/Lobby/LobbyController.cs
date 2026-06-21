using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Selfaware.Features.Game.Lobby.DTOs;
using Selfaware.Shared.Models;
using System.Security.Claims;

namespace Selfaware.Features.Game.Lobby
{
    [ApiController]
    [Route("api/[controller]")]
    public class LobbyController:ControllerBase
    {
        private readonly ILobbyService _lobbyService;

        public LobbyController(ILobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> GetLobby()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(CustomResponse<Guid>.ErrorResponse("Your have no permission for this action"));

            var result = await _lobbyService.GetLobbyAsync(userId);
            return Ok(CustomResponse<GetLobbyDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateLobby()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(CustomResponse<Guid>.ErrorResponse("Your have no permission for this action"));

            var result = await _lobbyService.CreateLobbyAsync(userId);

            return Ok(CustomResponse<string>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<ActionResult> DeleteLobby()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(CustomResponse<Guid>.ErrorResponse("Your have no permission for this action"));

            var result = await _lobbyService.DeleteLobbyAsync(userId);

            return Ok(CustomResponse<string>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles ="Admin")]
        [HttpDelete("players")]
        public async Task<ActionResult> KickLobbyPlayer([FromBody] KickLobbyPlayerDto dto)
        {
           
            var result = await _lobbyService.KickLobbyPlayerAsync(dto);

            return Ok(CustomResponse<string>.SuccessResponse(result.Data, result.Message));
        }

        /// player staff////
       

        [HttpPost("players")]
        public async Task<ActionResult> JoinLobby([FromBody] JoinLobbyDto dto)
        {
            if(dto.NickName == null)
            {
                return BadRequest(CustomResponse<string>.ErrorResponse("Enter nickName to join lobby"));
            }

            var result = await _lobbyService.JoinLobbyAsync(dto);


            return Ok(CustomResponse<GetLobbyPlayerDto>.SuccessResponse(result.Data, result.Message));
        }

        [HttpGet("{joinCode}/{playerId:guid}")]
        public async Task<ActionResult> GetLobbyForPlayer([FromRoute] string joinCode, Guid playerId)
        {

            var result = await _lobbyService.GetLobbyForPlayerAsync(joinCode, playerId);

            return Ok(CustomResponse<GetLobbyForPlayerDto>.SuccessResponse(result.Data, "Lobby found succesfully"));

        }



    }
}

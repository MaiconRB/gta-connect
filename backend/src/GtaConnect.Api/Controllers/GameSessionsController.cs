using GtaConnect.Api.Common;
using GtaConnect.Application.Features.Reputation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/game-sessions")]
[Authorize]
public class GameSessionsController : ControllerBase
{
    private readonly IGameSessionService _gameSessionService;

    public GameSessionsController(IGameSessionService gameSessionService)
    {
        _gameSessionService = gameSessionService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> LogSession(LogGameSessionRequestDto request, CancellationToken cancellationToken)
    {
        var sessionId = await _gameSessionService.LogSessionAsync(User.GetUserId(), request.ConnectionId, cancellationToken);
        return Ok(sessionId);
    }
}

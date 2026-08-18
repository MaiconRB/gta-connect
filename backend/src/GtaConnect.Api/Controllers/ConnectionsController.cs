using GtaConnect.Api.Common;
using GtaConnect.Application.Features.Reputation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/connections")]
[Authorize]
public class ConnectionsController : ControllerBase
{
    private readonly IConnectionService _connectionService;

    public ConnectionsController(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    [HttpPost]
    public async Task<ActionResult<ConnectionSummaryDto>> SendRequest(ConnectionRequestDto request, CancellationToken cancellationToken)
    {
        var connection = await _connectionService.SendRequestAsync(User.GetUserId(), request.ProfileId, cancellationToken);
        return Ok(this.WithAbsoluteAvatarUrl(connection));
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        await _connectionService.AcceptAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id, CancellationToken cancellationToken)
    {
        await _connectionService.DeclineAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConnectionSummaryDto>>> GetConnections(CancellationToken cancellationToken)
    {
        var connections = await _connectionService.GetConnectionsAsync(User.GetUserId(), cancellationToken);
        return Ok(connections.Select(this.WithAbsoluteAvatarUrl).ToList());
    }

    [HttpGet("with/{profileId:guid}")]
    public async Task<ActionResult<ConnectionStatusDto>> GetConnectionStatus(Guid profileId, CancellationToken cancellationToken)
    {
        var status = await _connectionService.GetConnectionStatusAsync(User.GetUserId(), profileId, cancellationToken);
        return Ok(status);
    }
}

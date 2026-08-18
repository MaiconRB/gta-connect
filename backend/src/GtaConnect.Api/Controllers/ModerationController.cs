using GtaConnect.Api.Common;
using GtaConnect.Application.Features.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/moderation")]
[Authorize]
public class ModerationController : ControllerBase
{
    private readonly IModerationService _moderationService;

    public ModerationController(IModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    [HttpPost("blocks")]
    public async Task<IActionResult> Block(BlockRequestDto request, CancellationToken cancellationToken)
    {
        await _moderationService.BlockAsync(User.GetUserId(), request.ProfileId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("blocks/{profileId:guid}")]
    public async Task<IActionResult> Unblock(Guid profileId, CancellationToken cancellationToken)
    {
        await _moderationService.UnblockAsync(User.GetUserId(), profileId, cancellationToken);
        return NoContent();
    }

    [HttpGet("blocks")]
    public async Task<ActionResult<IReadOnlyList<BlockedProfileSummaryDto>>> GetBlockedProfiles(CancellationToken cancellationToken)
    {
        var blockedProfiles = await _moderationService.GetBlockedProfilesAsync(User.GetUserId(), cancellationToken);
        return Ok(blockedProfiles.Select(this.WithAbsoluteAvatarUrl).ToList());
    }

    [HttpPost("reports")]
    public async Task<IActionResult> Report(ReportRequestDto request, CancellationToken cancellationToken)
    {
        await _moderationService.ReportAsync(User.GetUserId(), request.ProfileId, request.Reason, request.Details, cancellationToken);
        return NoContent();
    }
}

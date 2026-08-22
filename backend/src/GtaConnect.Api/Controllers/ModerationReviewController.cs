using GtaConnect.Api.Common;
using GtaConnect.Application.Features.ModerationReview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/moderation/review")]
[Authorize(Roles = "Moderator")]
public class ModerationReviewController : ControllerBase
{
    private readonly IModerationReviewService _moderationReviewService;

    public ModerationReviewController(IModerationReviewService moderationReviewService)
    {
        _moderationReviewService = moderationReviewService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReportedProfileSummaryDto>>> GetReportedProfiles(CancellationToken cancellationToken)
    {
        var profiles = await _moderationReviewService.GetReportedProfilesAsync(cancellationToken);
        return Ok(profiles.Select(this.WithAbsoluteAvatarUrl).ToList());
    }

    [HttpGet("{profileId:guid}")]
    public async Task<ActionResult<ReportedProfileDetailDto>> GetReportedProfileDetail(Guid profileId, CancellationToken cancellationToken)
    {
        var detail = await _moderationReviewService.GetReportedProfileDetailAsync(profileId, cancellationToken);
        return Ok(this.WithAbsoluteAvatarUrl(detail));
    }

    [HttpPost("reports/{reportId:guid}/mark-reviewed")]
    public async Task<IActionResult> MarkReportReviewed(Guid reportId, CancellationToken cancellationToken)
    {
        await _moderationReviewService.MarkReportReviewedAsync(User.GetUserId(), reportId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{profileId:guid}/ban")]
    public async Task<IActionResult> Ban(Guid profileId, CancellationToken cancellationToken)
    {
        await _moderationReviewService.BanProfileAsync(profileId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{profileId:guid}/unban")]
    public async Task<IActionResult> Unban(Guid profileId, CancellationToken cancellationToken)
    {
        await _moderationReviewService.UnbanProfileAsync(profileId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("posts/{postId:guid}")]
    public async Task<IActionResult> DeletePost(Guid postId, CancellationToken cancellationToken)
    {
        await _moderationReviewService.DeletePostAsModeratorAsync(postId, cancellationToken);
        return NoContent();
    }
}

using GtaConnect.Api.Common;
using GtaConnect.Application.Common;
using GtaConnect.Application.Features.Feed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly IFeedService _feedService;

    public FeedController(IFeedService feedService)
    {
        _feedService = feedService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<PostSummaryDto>>> GetFeed(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] bool onlyConnections,
        CancellationToken cancellationToken)
    {
        var result = await _feedService.GetFeedAsync(User.GetUserId(), page, pageSize, onlyConnections, cancellationToken);
        return Ok(result with { Items = result.Items.Select(this.WithAbsoluteAvatarUrl).ToList() });
    }

    // Guarda-corpo real de tamanho — sem isso, o Kestrel/FormOptions aceitariam um upload
    // bem maior antes de qualquer validação de código (tipo/tamanho) rodar na Application.
    [HttpPost("posts")]
    [RequestSizeLimit(2 * 1024 * 1024 + 1024)]
    public async Task<ActionResult<PostSummaryDto>> CreatePost([FromForm] string? content, IFormFile? photo, CancellationToken cancellationToken)
    {
        await using var photoStream = photo is { Length: > 0 } ? photo.OpenReadStream() : null;
        var post = await _feedService.CreatePostAsync(User.GetUserId(), content, photoStream, photo?.FileName, photo?.Length ?? 0, cancellationToken);
        return Ok(this.WithAbsoluteAvatarUrl(post));
    }

    [HttpDelete("posts/{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken)
    {
        await _feedService.DeletePostAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPost("posts/{id:guid}/like")]
    public async Task<ActionResult<LikeToggleResultDto>> ToggleLike(Guid id, CancellationToken cancellationToken)
    {
        var result = await _feedService.ToggleLikeAsync(User.GetUserId(), id, cancellationToken);
        return Ok(result);
    }
}

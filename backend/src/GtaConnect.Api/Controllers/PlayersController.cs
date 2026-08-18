using GtaConnect.Api.Common;
using GtaConnect.Application.Common;
using GtaConnect.Application.Features.PlayerSearch;
using GtaConnect.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/players")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly IPlayerSearchService _playerSearchService;

    public PlayersController(IPlayerSearchService playerSearchService)
    {
        _playerSearchService = playerSearchService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<PlayerSummaryDto>>> Search(
        [FromQuery] Platform? platform,
        [FromQuery] PlaystyleTag playstyleTags,
        [FromQuery] Region? region,
        [FromQuery] AvailabilityTag availabilityTags,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var filter = new PlayerSearchFilterDto(platform, playstyleTags, region, availabilityTags, page, pageSize);
        var result = await _playerSearchService.SearchAsync(User.GetUserId(), filter, cancellationToken);

        return Ok(result with { Items = result.Items.Select(this.WithAbsoluteAvatarUrl).ToList() });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlayerSummaryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var summary = await _playerSearchService.GetPlayerProfileAsync(id, cancellationToken);
        return Ok(this.WithAbsoluteAvatarUrl(summary));
    }
}

using GtaConnect.Api.Common;
using GtaConnect.Application.Features.Reputation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/ratings")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    public async Task<IActionResult> Rate(RatingRequestDto request, CancellationToken cancellationToken)
    {
        await _ratingService.RateAsync(User.GetUserId(), request.ProfileId, request.Score, request.Comment, cancellationToken);
        return NoContent();
    }
}

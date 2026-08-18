namespace GtaConnect.Application.Features.PlayerSearch;

public interface IPlayerSearchService
{
    Task<PagedResultDto<PlayerSummaryDto>> SearchAsync(Guid currentUserId, PlayerSearchFilterDto filter, CancellationToken cancellationToken = default);

    Task<PlayerSummaryDto> GetPlayerProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}

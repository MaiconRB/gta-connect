using GtaConnect.Application.Common;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Features.PlayerSearch;

public class PlayerSearchService : IPlayerSearchService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;

    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IBlockRepository _blockRepository;
    private readonly IRatingRepository _ratingRepository;

    public PlayerSearchService(IPlayerProfileRepository playerProfileRepository, IBlockRepository blockRepository, IRatingRepository ratingRepository)
    {
        _playerProfileRepository = playerProfileRepository;
        _blockRepository = blockRepository;
        _ratingRepository = ratingRepository;
    }

    public async Task<PagedResultDto<PlayerSummaryDto>> SearchAsync(Guid currentUserId, PlayerSearchFilterDto filter, CancellationToken cancellationToken = default)
    {
        // Nunca confiar em Page/PageSize crus vindos da query string — clampar aqui,
        // não no controller, é regra de aplicação (não de transporte HTTP).
        var clampedFilter = filter with
        {
            Page = filter.Page < 1 ? 1 : filter.Page,
            PageSize = filter.PageSize < 1 ? DefaultPageSize : Math.Min(filter.PageSize, MaxPageSize),
        };

        var myProfile = await _playerProfileRepository.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), currentUserId);

        // Nunca aparece o próprio usuário nem quem estiver bloqueado (nos dois sentidos) na busca.
        var blockedOrBlockingIds = await _blockRepository.GetBlockedOrBlockingProfileIdsAsync(myProfile.Id, cancellationToken);
        var excludedProfileIds = new HashSet<Guid>(blockedOrBlockingIds) { myProfile.Id };

        var (items, totalCount) = await _playerProfileRepository.SearchAsync(clampedFilter, excludedProfileIds, cancellationToken);

        // Uma query só pros agregados de avaliação de todo mundo da página atual — evita N+1.
        var aggregates = await _ratingRepository.GetAggregatesAsync(items.Select(p => p.Id).ToList(), cancellationToken);

        return new PagedResultDto<PlayerSummaryDto>(
            items.Select(p => ToSummaryDto(p, aggregates)).ToList(),
            totalCount,
            clampedFilter.Page,
            clampedFilter.PageSize);
    }

    public async Task<PlayerSummaryDto> GetPlayerProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var profile = await _playerProfileRepository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), profileId);

        var aggregates = await _ratingRepository.GetAggregatesAsync([profile.Id], cancellationToken);

        return ToSummaryDto(profile, aggregates);
    }

    private static PlayerSummaryDto ToSummaryDto(PlayerProfile profile, IReadOnlyDictionary<Guid, (double Average, int Count)> aggregates)
    {
        var hasAggregate = aggregates.TryGetValue(profile.Id, out var aggregate);

        return new PlayerSummaryDto(
            profile.Id,
            profile.DisplayName,
            profile.Platform,
            profile.GameTitle,
            profile.Bio,
            profile.PlaystyleTags,
            profile.AvailabilityTags,
            profile.Region,
            profile.HoursPlayed,
            profile.FavoriteModes,
            profile.AvatarPath,
            profile.CreatedAtUtc,
            hasAggregate ? aggregate.Average : null,
            hasAggregate ? aggregate.Count : 0);
    }
}

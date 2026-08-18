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

    public PlayerSearchService(IPlayerProfileRepository playerProfileRepository, IBlockRepository blockRepository)
    {
        _playerProfileRepository = playerProfileRepository;
        _blockRepository = blockRepository;
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

        return new PagedResultDto<PlayerSummaryDto>(
            items.Select(ToSummaryDto).ToList(),
            totalCount,
            clampedFilter.Page,
            clampedFilter.PageSize);
    }

    public async Task<PlayerSummaryDto> GetPlayerProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var profile = await _playerProfileRepository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), profileId);

        return ToSummaryDto(profile);
    }

    private static PlayerSummaryDto ToSummaryDto(PlayerProfile profile) => new(
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
        profile.CreatedAtUtc);
}

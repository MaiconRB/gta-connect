using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Moderation;

public class ModerationService : IModerationService
{
    private readonly IBlockRepository _blockRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IPlayerProfileRepository _playerProfileRepository;

    public ModerationService(IBlockRepository blockRepository, IReportRepository reportRepository, IPlayerProfileRepository playerProfileRepository)
    {
        _blockRepository = blockRepository;
        _reportRepository = reportRepository;
        _playerProfileRepository = playerProfileRepository;
    }

    public async Task BlockAsync(Guid userId, Guid targetProfileId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        await EnsureTargetExistsAsync(targetProfileId, cancellationToken);

        var existing = await _blockRepository.FindAsync(myProfile.Id, targetProfileId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var block = Block.Create(myProfile.Id, targetProfileId);
        await _blockRepository.AddAsync(block, cancellationToken);
    }

    public async Task UnblockAsync(Guid userId, Guid targetProfileId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var existing = await _blockRepository.FindAsync(myProfile.Id, targetProfileId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        await _blockRepository.RemoveAsync(existing, cancellationToken);
    }

    public async Task<IReadOnlyList<BlockedProfileSummaryDto>> GetBlockedProfilesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        return await _blockRepository.GetBlockedProfilesAsync(myProfile.Id, cancellationToken);
    }

    public async Task ReportAsync(Guid userId, Guid targetProfileId, ReportReason reason, string? details, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        await EnsureTargetExistsAsync(targetProfileId, cancellationToken);

        var report = Report.Create(myProfile.Id, targetProfileId, reason, details);
        await _reportRepository.AddAsync(report, cancellationToken);
    }

    private async Task<PlayerProfile> GetProfileOrThrowAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);
    }

    private async Task EnsureTargetExistsAsync(Guid targetProfileId, CancellationToken cancellationToken)
    {
        _ = await _playerProfileRepository.GetByIdAsync(targetProfileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), targetProfileId);
    }
}

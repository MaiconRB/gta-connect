using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Moderation;
using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class BlockRepository : IBlockRepository
{
    private readonly AppDbContext _dbContext;

    public BlockRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsEitherDirectionAsync(Guid profileAId, Guid profileBId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Blocks.AnyAsync(
            b => (b.BlockerProfileId == profileAId && b.BlockedProfileId == profileBId)
                || (b.BlockerProfileId == profileBId && b.BlockedProfileId == profileAId),
            cancellationToken);
    }

    public Task<Block?> FindAsync(Guid blockerProfileId, Guid blockedProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Blocks
            .SingleOrDefaultAsync(b => b.BlockerProfileId == blockerProfileId && b.BlockedProfileId == blockedProfileId, cancellationToken);
    }

    public async Task AddAsync(Block block, CancellationToken cancellationToken = default)
    {
        _dbContext.Blocks.Add(block);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Block block, CancellationToken cancellationToken = default)
    {
        _dbContext.Blocks.Remove(block);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetBlockedOrBlockingProfileIdsAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var asBlocker = _dbContext.Blocks.Where(b => b.BlockerProfileId == profileId).Select(b => b.BlockedProfileId);
        var asBlocked = _dbContext.Blocks.Where(b => b.BlockedProfileId == profileId).Select(b => b.BlockerProfileId);

        return await asBlocker.Union(asBlocked).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BlockedProfileSummaryDto>> GetBlockedProfilesAsync(Guid blockerProfileId, CancellationToken cancellationToken = default)
    {
        var query =
            from b in _dbContext.Blocks
            where b.BlockerProfileId == blockerProfileId
            join blocked in _dbContext.PlayerProfiles on b.BlockedProfileId equals blocked.Id
            orderby b.CreatedAtUtc descending
            select new BlockedProfileSummaryDto(blocked.Id, blocked.DisplayName, blocked.AvatarPath, b.CreatedAtUtc);

        return await query.ToListAsync(cancellationToken);
    }
}

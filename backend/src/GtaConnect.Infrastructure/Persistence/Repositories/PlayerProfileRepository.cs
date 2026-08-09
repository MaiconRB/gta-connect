using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class PlayerProfileRepository : IPlayerProfileRepository
{
    private readonly AppDbContext _dbContext;

    public PlayerProfileRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        _dbContext.PlayerProfiles.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<PlayerProfile?> GetByUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PlayerProfiles
            .SingleOrDefaultAsync(p => p.ApplicationUserId == applicationUserId, cancellationToken);
    }

    public async Task UpdateAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        _dbContext.PlayerProfiles.Update(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

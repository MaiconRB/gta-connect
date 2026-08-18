using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _dbContext;

    public RatingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Rating?> FindAsync(Guid raterProfileId, Guid ratedProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Ratings.SingleOrDefaultAsync(r => r.RaterProfileId == raterProfileId && r.RatedProfileId == ratedProfileId, cancellationToken);
    }

    public async Task AddAsync(Rating rating, CancellationToken cancellationToken = default)
    {
        _dbContext.Ratings.Add(rating);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Rating rating, CancellationToken cancellationToken = default)
    {
        // "rating" já vem rastreado (veio de FindAsync, mesmo DbContext/escopo) — nada de
        // .Update() aqui (mesmo motivo já documentado em ChatRepository/ConnectionRepository).
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, (double Average, int Count)>> GetAggregatesAsync(
        IReadOnlyCollection<Guid> profileIds,
        CancellationToken cancellationToken = default)
    {
        var results = await _dbContext.Ratings
            .Where(r => profileIds.Contains(r.RatedProfileId))
            .GroupBy(r => r.RatedProfileId)
            .Select(g => new { ProfileId = g.Key, Average = g.Average(r => r.Score), Count = g.Count() })
            .ToListAsync(cancellationToken);

        return results.ToDictionary(r => r.ProfileId, r => (r.Average, r.Count));
    }
}

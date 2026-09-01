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

    public Task<Rating?> FindBySessionAsync(Guid gameSessionId, Guid raterProfileId, Guid ratedProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Ratings.SingleOrDefaultAsync(
            r => r.GameSessionId == gameSessionId && r.RaterProfileId == raterProfileId && r.RatedProfileId == ratedProfileId,
            cancellationToken);
    }

    public async Task AddAsync(Rating rating, CancellationToken cancellationToken = default)
    {
        _dbContext.Ratings.Add(rating);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Rating rating, CancellationToken cancellationToken = default)
    {
        // "rating" já vem rastreado (veio de FindBySessionAsync, mesmo DbContext/escopo) — nada
        // de .Update() aqui (mesmo motivo já documentado em ChatRepository/ConnectionRepository).
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, RatingAggregate>> GetAggregatesAsync(
        IReadOnlyCollection<Guid> profileIds,
        CancellationToken cancellationToken = default)
    {
        var results = await _dbContext.Ratings
            .Where(r => profileIds.Contains(r.RatedProfileId))
            .GroupBy(r => r.RatedProfileId)
            .Select(g => new
            {
                ProfileId = g.Key,
                Average = g.Average(r => r.Score),
                Count = g.Count(),
                // Fração de avaliações recebidas marcadas como sessão concluída — traduzível
                // pro SQL Server como AVG(CASE WHEN ... THEN 1.0 ELSE 0.0 END), sem trazer as
                // linhas pra memória antes de agregar.
                CompletionRate = g.Average(r => r.CompletedSession ? 1.0 : 0.0),
            })
            .ToListAsync(cancellationToken);

        return results.ToDictionary(r => r.ProfileId, r => new RatingAggregate(r.Average, r.Count, r.CompletionRate));
    }
}

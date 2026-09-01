using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly AppDbContext _dbContext;

    public GameSessionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GameSession?> GetByIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.GameSessions.SingleOrDefaultAsync(s => s.Id == gameSessionId, cancellationToken);
    }

    public async Task AddAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        _dbContext.GameSessions.Add(gameSession);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<GameSession?> GetLatestForConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.GameSessions
            .Where(s => s.ConnectionId == connectionId)
            .OrderByDescending(s => s.PlayedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

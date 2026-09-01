using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(Guid gameSessionId, CancellationToken cancellationToken = default);

    Task AddAsync(GameSession gameSession, CancellationToken cancellationToken = default);

    /// <summary>Sessão mais recente registrada pra essa Connection, ou null se nunca jogaram.</summary>
    Task<GameSession?> GetLatestForConnectionAsync(Guid connectionId, CancellationToken cancellationToken = default);
}

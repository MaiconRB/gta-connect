namespace GtaConnect.Application.Features.Reputation;

public interface IGameSessionService
{
    /// <summary>Registra "jogamos juntos agora" pra uma conexão aceita. Devolve o id da sessão criada.</summary>
    Task<Guid> LogSessionAsync(Guid userId, Guid connectionId, CancellationToken cancellationToken = default);
}

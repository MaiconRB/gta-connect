namespace GtaConnect.Application.Features.Reputation;

public interface IConnectionService
{
    /// <summary>Idempotente-ish: pedido pendente/aceito já existente é devolvido em vez de duplicado.</summary>
    Task<ConnectionSummaryDto> SendRequestAsync(Guid userId, Guid targetProfileId, CancellationToken cancellationToken = default);

    /// <summary>Só quem recebeu o pedido pode aceitar.</summary>
    Task AcceptAsync(Guid userId, Guid connectionId, CancellationToken cancellationToken = default);

    /// <summary>Quem recebeu (recusando) ou quem enviou (cancelando) pode chamar.</summary>
    Task DeclineAsync(Guid userId, Guid connectionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConnectionSummaryDto>> GetConnectionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Status da conexão + minha própria nota, relativos a um perfil específico.</summary>
    Task<ConnectionStatusDto> GetConnectionStatusAsync(Guid userId, Guid targetProfileId, CancellationToken cancellationToken = default);
}

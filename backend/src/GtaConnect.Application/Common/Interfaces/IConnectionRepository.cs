using GtaConnect.Application.Features.Reputation;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

public interface IConnectionRepository
{
    /// <summary>Busca uma Connection entre os dois perfis, em qualquer direção e qualquer status.</summary>
    Task<Connection?> FindBetweenAsync(Guid profileAId, Guid profileBId, CancellationToken cancellationToken = default);

    Task<Connection?> GetByIdAsync(Guid connectionId, CancellationToken cancellationToken = default);

    Task AddAsync(Connection connection, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(Connection connection, CancellationToken cancellationToken = default);

    /// <summary>Todas as conexões do perfil (qualquer status/direção), já com o outro participante resolvido.</summary>
    Task<IReadOnlyList<ConnectionSummaryDto>> GetConnectionsForProfileAsync(Guid profileId, CancellationToken cancellationToken = default);
}

using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

/// <summary>
/// Interface específica para PlayerProfile — de propósito, NÃO um IRepository&lt;T&gt; genérico.
/// O EF Core (DbContext) já é o Repository + Unit of Work; abstrações genéricas em cima dele
/// só adicionam indireção sem ganho real num projeto deste porte.
/// </summary>
public interface IPlayerProfileRepository
{
    Task AddAsync(PlayerProfile profile, CancellationToken cancellationToken = default);

    Task<PlayerProfile?> GetByUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);

    Task UpdateAsync(PlayerProfile profile, CancellationToken cancellationToken = default);
}

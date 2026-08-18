using GtaConnect.Application.Features.PlayerSearch;
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

    /// <summary>Abre o perfil de QUALQUER jogador pelo Id do perfil — diferente de GetByUserIdAsync, que é pelo Id do usuário logado (via JWT).</summary>
    Task<PlayerProfile?> GetByIdAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task UpdateAsync(PlayerProfile profile, CancellationToken cancellationToken = default);

    /// <summary>Busca paginada com filtros opcionais. excludeProfileId nunca aparece nos resultados (é sempre o perfil do usuário logado).</summary>
    Task<(IReadOnlyList<PlayerProfile> Items, int TotalCount)> SearchAsync(PlayerSearchFilterDto filter, Guid excludeProfileId, CancellationToken cancellationToken = default);
}

using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

public interface IRatingRepository
{
    Task<Rating?> FindAsync(Guid raterProfileId, Guid ratedProfileId, CancellationToken cancellationToken = default);

    Task AddAsync(Rating rating, CancellationToken cancellationToken = default);

    Task UpdateAsync(Rating rating, CancellationToken cancellationToken = default);

    /// <summary>Média e contagem de avaliações, em lote, só pros perfis pedidos. Perfil sem avaliação nenhuma não aparece no dicionário.</summary>
    Task<IReadOnlyDictionary<Guid, (double Average, int Count)>> GetAggregatesAsync(
        IReadOnlyCollection<Guid> profileIds,
        CancellationToken cancellationToken = default);
}

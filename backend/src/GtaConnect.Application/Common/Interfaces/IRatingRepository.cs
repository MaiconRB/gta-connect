using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

public interface IRatingRepository
{
    /// <summary>Uma avaliação é única por (sessão, avaliador, avaliado) — não mais por par sozinho.</summary>
    Task<Rating?> FindBySessionAsync(Guid gameSessionId, Guid raterProfileId, Guid ratedProfileId, CancellationToken cancellationToken = default);

    Task AddAsync(Rating rating, CancellationToken cancellationToken = default);

    Task UpdateAsync(Rating rating, CancellationToken cancellationToken = default);

    /// <summary>
    /// Média de nota, contagem e taxa de conclusão (fração das avaliações recebidas com
    /// CompletedSession=true), em lote, só pros perfis pedidos. Perfil sem avaliação nenhuma
    /// não aparece no dicionário — quem consome decide o valor neutro pra ausência de dado.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, RatingAggregate>> GetAggregatesAsync(
        IReadOnlyCollection<Guid> profileIds,
        CancellationToken cancellationToken = default);
}

public readonly record struct RatingAggregate(double Average, int Count, double CompletionRate);

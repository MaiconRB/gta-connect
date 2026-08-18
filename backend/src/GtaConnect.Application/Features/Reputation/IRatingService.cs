namespace GtaConnect.Application.Features.Reputation;

public interface IRatingService
{
    /// <summary>Upsert — se já existe uma avaliação minha pra esse perfil, atualiza em vez de duplicar.</summary>
    Task RateAsync(Guid userId, Guid targetProfileId, int score, string? comment, CancellationToken cancellationToken = default);
}

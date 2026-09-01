namespace GtaConnect.Application.Features.Reputation;

public interface IRatingService
{
    /// <summary>
    /// Upsert — se já existe uma avaliação minha pra essa sessão, atualiza em vez de duplicar.
    /// O avaliado é resolvido a partir da sessão (o outro participante da Connection dela),
    /// não é mais passado direto pelo cliente.
    /// </summary>
    Task RateAsync(
        Guid userId,
        Guid gameSessionId,
        int score,
        string? comment,
        bool completedSession,
        bool knewWhatToDo,
        bool wasToxic,
        CancellationToken cancellationToken = default);
}

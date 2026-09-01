namespace GtaConnect.Domain.Entities;

// Rating agora é por SESSÃO (GameSessionId), não mais um upsert único por par —
// é o que permite responder "terminou 12 de 12 sessões" em vez de só uma impressão
// geral sobrescrita a cada nova avaliação. As flags de confiabilidade (completou,
// sabia jogar, foi tóxico) existem ao lado da nota de 1-5, não no lugar dela — a nota
// captura impressão geral, as flags capturam o sinal estruturado que alimenta o score
// de compatibilidade e, mais tarde, o modelo de ML.
public class Rating
{
    private const int MinScore = 1;
    private const int MaxScore = 5;
    private const int MaxCommentLength = 500;

    public Guid Id { get; private set; }

    public Guid GameSessionId { get; private set; }

    public Guid RaterProfileId { get; private set; }

    public Guid RatedProfileId { get; private set; }

    public int Score { get; private set; }

    public string? Comment { get; private set; }

    public bool CompletedSession { get; private set; }

    public bool KnewWhatToDo { get; private set; }

    public bool WasToxic { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Rating()
    {
    }

    public static Rating Create(
        Guid gameSessionId,
        Guid raterProfileId,
        Guid ratedProfileId,
        int score,
        string? comment,
        bool completedSession,
        bool knewWhatToDo,
        bool wasToxic)
    {
        if (gameSessionId == Guid.Empty || raterProfileId == Guid.Empty || ratedProfileId == Guid.Empty)
        {
            throw new ArgumentException("Sessão e perfis da avaliação não podem ser vazios.");
        }

        if (raterProfileId == ratedProfileId)
        {
            throw new ArgumentException("Não é possível avaliar a si mesmo.");
        }

        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            RaterProfileId = raterProfileId,
            RatedProfileId = ratedProfileId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        rating.SetFields(score, comment, completedSession, knewWhatToDo, wasToxic);
        return rating;
    }

    public void Update(int score, string? comment, bool completedSession, bool knewWhatToDo, bool wasToxic)
    {
        SetFields(score, comment, completedSession, knewWhatToDo, wasToxic);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetFields(int score, string? comment, bool completedSession, bool knewWhatToDo, bool wasToxic)
    {
        if (score is < MinScore or > MaxScore)
        {
            throw new ArgumentOutOfRangeException(nameof(score), $"A nota deve estar entre {MinScore} e {MaxScore}.");
        }

        var trimmedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (trimmedComment is { Length: > MaxCommentLength })
        {
            throw new ArgumentException($"O comentário não pode ter mais de {MaxCommentLength} caracteres.", nameof(comment));
        }

        Score = score;
        Comment = trimmedComment;
        CompletedSession = completedSession;
        KnewWhatToDo = knewWhatToDo;
        WasToxic = wasToxic;
    }
}

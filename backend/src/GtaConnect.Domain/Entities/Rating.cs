namespace GtaConnect.Domain.Entities;

public class Rating
{
    private const int MinScore = 1;
    private const int MaxScore = 5;
    private const int MaxCommentLength = 500;

    public Guid Id { get; private set; }

    public Guid RaterProfileId { get; private set; }

    public Guid RatedProfileId { get; private set; }

    public int Score { get; private set; }

    public string? Comment { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Rating()
    {
    }

    public static Rating Create(Guid raterProfileId, Guid ratedProfileId, int score, string? comment)
    {
        if (raterProfileId == Guid.Empty || ratedProfileId == Guid.Empty)
        {
            throw new ArgumentException("Os perfis da avaliação não podem ser vazios.");
        }

        if (raterProfileId == ratedProfileId)
        {
            throw new ArgumentException("Não é possível avaliar a si mesmo.");
        }

        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            RaterProfileId = raterProfileId,
            RatedProfileId = ratedProfileId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        rating.SetScoreAndComment(score, comment);
        return rating;
    }

    public void Update(int score, string? comment)
    {
        SetScoreAndComment(score, comment);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetScoreAndComment(int score, string? comment)
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
    }
}

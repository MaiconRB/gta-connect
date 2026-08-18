namespace GtaConnect.Domain.Entities;

public class Post
{
    private const int MaxContentLength = 1000;

    public Guid Id { get; private set; }

    public Guid AuthorProfileId { get; private set; }

    public string? Content { get; private set; }

    public string? PhotoPath { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Post()
    {
    }

    public static Post Create(Guid authorProfileId, string? content, string? photoPath)
    {
        if (authorProfileId == Guid.Empty)
        {
            throw new ArgumentException("O autor do post não pode ser vazio.");
        }

        var trimmedContent = string.IsNullOrWhiteSpace(content) ? null : content.Trim();
        var trimmedPhotoPath = string.IsNullOrWhiteSpace(photoPath) ? null : photoPath;

        if (trimmedContent is null && trimmedPhotoPath is null)
        {
            throw new ArgumentException("O post precisa ter texto ou foto.");
        }

        if (trimmedContent is { Length: > MaxContentLength })
        {
            throw new ArgumentException($"O texto do post não pode ter mais de {MaxContentLength} caracteres.", nameof(content));
        }

        return new Post
        {
            Id = Guid.NewGuid(),
            AuthorProfileId = authorProfileId,
            Content = trimmedContent,
            PhotoPath = trimmedPhotoPath,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}

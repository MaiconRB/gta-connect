namespace GtaConnect.Domain.Entities;

public class PostLike
{
    public Guid Id { get; private set; }

    public Guid PostId { get; private set; }

    public Guid ProfileId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private PostLike()
    {
    }

    public static PostLike Create(Guid postId, Guid profileId)
    {
        if (postId == Guid.Empty || profileId == Guid.Empty)
        {
            throw new ArgumentException("Post e perfil da curtida não podem ser vazios.");
        }

        return new PostLike
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            ProfileId = profileId,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}

using GtaConnect.Domain.Entities;

namespace GtaConnect.Domain.Tests.Entities;

public class PostLikeTests
{
    [Fact]
    public void Create_ComDadosValidos_CriaPostLike()
    {
        var postId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        var like = PostLike.Create(postId, profileId);

        Assert.Equal(postId, like.PostId);
        Assert.Equal(profileId, like.ProfileId);
    }

    [Fact]
    public void Create_ComPostIdVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PostLike.Create(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Create_ComProfileIdVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PostLike.Create(Guid.NewGuid(), Guid.Empty));
    }
}

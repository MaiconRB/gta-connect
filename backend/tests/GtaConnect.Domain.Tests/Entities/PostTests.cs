using GtaConnect.Domain.Entities;

namespace GtaConnect.Domain.Tests.Entities;

public class PostTests
{
    [Fact]
    public void Create_ComSoTexto_CriaPostComContentAparado()
    {
        var authorProfileId = Guid.NewGuid();

        var post = Post.Create(authorProfileId, "  finalmente terminei a campanha!  ", null);

        Assert.Equal(authorProfileId, post.AuthorProfileId);
        Assert.Equal("finalmente terminei a campanha!", post.Content);
        Assert.Null(post.PhotoPath);
    }

    [Fact]
    public void Create_ComSoFoto_CriaPostSemContent()
    {
        var post = Post.Create(Guid.NewGuid(), null, "/uploads/posts/abc.jpg");

        Assert.Null(post.Content);
        Assert.Equal("/uploads/posts/abc.jpg", post.PhotoPath);
    }

    [Fact]
    public void Create_ComTextoEFoto_CriaPostComOsDois()
    {
        var post = Post.Create(Guid.NewGuid(), "Cayo Perico solo", "/uploads/posts/abc.jpg");

        Assert.Equal("Cayo Perico solo", post.Content);
        Assert.Equal("/uploads/posts/abc.jpg", post.PhotoPath);
    }

    [Fact]
    public void Create_SemTextoNemFoto_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Post.Create(Guid.NewGuid(), null, null));
    }

    [Fact]
    public void Create_ComTextoSoEspacos_TrataComoVazio()
    {
        Assert.Throws<ArgumentException>(() => Post.Create(Guid.NewGuid(), "   ", null));
    }

    [Fact]
    public void Create_ComAutorVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Post.Create(Guid.Empty, "Oi", null));
    }

    [Fact]
    public void Create_ComTextoMuitoLongo_LancaArgumentException()
    {
        var tooLong = new string('a', 1001);

        Assert.Throws<ArgumentException>(() => Post.Create(Guid.NewGuid(), tooLong, null));
    }
}

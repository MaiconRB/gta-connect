using GtaConnect.Domain.Entities;

namespace GtaConnect.Domain.Tests.Entities;

public class RatingTests
{
    [Fact]
    public void Create_ComDadosValidos_CriaRatingComComentarioAparado()
    {
        var raterProfileId = Guid.NewGuid();
        var ratedProfileId = Guid.NewGuid();

        var rating = Rating.Create(raterProfileId, ratedProfileId, 5, "  jogador excelente, recomendo!  ");

        Assert.Equal(raterProfileId, rating.RaterProfileId);
        Assert.Equal(ratedProfileId, rating.RatedProfileId);
        Assert.Equal(5, rating.Score);
        Assert.Equal("jogador excelente, recomendo!", rating.Comment);
        Assert.Null(rating.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_ComScoreForaDoIntervalo_LancaArgumentOutOfRangeException(int invalidScore)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Rating.Create(Guid.NewGuid(), Guid.NewGuid(), invalidScore, null));
    }

    [Fact]
    public void Create_ComMesmoPerfilNosDoisLados_LancaArgumentException()
    {
        var profileId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => Rating.Create(profileId, profileId, 5, null));
    }

    [Fact]
    public void Create_ComComentarioMuitoLongo_LancaArgumentException()
    {
        var tooLong = new string('a', 501);

        Assert.Throws<ArgumentException>(() => Rating.Create(Guid.NewGuid(), Guid.NewGuid(), 5, tooLong));
    }

    [Fact]
    public void Update_MudaScoreComentarioEUpdatedAtUtc()
    {
        var rating = Rating.Create(Guid.NewGuid(), Guid.NewGuid(), 3, "ok");

        rating.Update(5, "na verdade foi ótimo!");

        Assert.Equal(5, rating.Score);
        Assert.Equal("na verdade foi ótimo!", rating.Comment);
        Assert.NotNull(rating.UpdatedAtUtc);
    }

    [Fact]
    public void Update_ComScoreForaDoIntervalo_LancaArgumentOutOfRangeException()
    {
        var rating = Rating.Create(Guid.NewGuid(), Guid.NewGuid(), 3, null);

        Assert.Throws<ArgumentOutOfRangeException>(() => rating.Update(10, null));
    }
}

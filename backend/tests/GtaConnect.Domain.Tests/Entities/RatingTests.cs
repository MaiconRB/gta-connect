using GtaConnect.Domain.Entities;

namespace GtaConnect.Domain.Tests.Entities;

public class RatingTests
{
    private static Guid NewSessionId() => Guid.NewGuid();

    [Fact]
    public void Create_ComDadosValidos_CriaRatingComComentarioAparado()
    {
        var gameSessionId = NewSessionId();
        var raterProfileId = Guid.NewGuid();
        var ratedProfileId = Guid.NewGuid();

        var rating = Rating.Create(gameSessionId, raterProfileId, ratedProfileId, 5, "  jogador excelente, recomendo!  ", true, true, false);

        Assert.Equal(gameSessionId, rating.GameSessionId);
        Assert.Equal(raterProfileId, rating.RaterProfileId);
        Assert.Equal(ratedProfileId, rating.RatedProfileId);
        Assert.Equal(5, rating.Score);
        Assert.Equal("jogador excelente, recomendo!", rating.Comment);
        Assert.True(rating.CompletedSession);
        Assert.True(rating.KnewWhatToDo);
        Assert.False(rating.WasToxic);
        Assert.Null(rating.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_ComScoreForaDoIntervalo_LancaArgumentOutOfRangeException(int invalidScore)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Rating.Create(NewSessionId(), Guid.NewGuid(), Guid.NewGuid(), invalidScore, null, true, true, false));
    }

    [Fact]
    public void Create_ComMesmoPerfilNosDoisLados_LancaArgumentException()
    {
        var profileId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => Rating.Create(NewSessionId(), profileId, profileId, 5, null, true, true, false));
    }

    [Fact]
    public void Create_ComGameSessionIdVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Rating.Create(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 5, null, true, true, false));
    }

    [Fact]
    public void Create_ComComentarioMuitoLongo_LancaArgumentException()
    {
        var tooLong = new string('a', 501);

        Assert.Throws<ArgumentException>(() => Rating.Create(NewSessionId(), Guid.NewGuid(), Guid.NewGuid(), 5, tooLong, true, true, false));
    }

    [Fact]
    public void Update_MudaCamposEUpdatedAtUtc()
    {
        var rating = Rating.Create(NewSessionId(), Guid.NewGuid(), Guid.NewGuid(), 3, "ok", false, true, false);

        rating.Update(5, "na verdade foi ótimo!", true, true, false);

        Assert.Equal(5, rating.Score);
        Assert.Equal("na verdade foi ótimo!", rating.Comment);
        Assert.True(rating.CompletedSession);
        Assert.NotNull(rating.UpdatedAtUtc);
    }

    [Fact]
    public void Update_ComScoreForaDoIntervalo_LancaArgumentOutOfRangeException()
    {
        var rating = Rating.Create(NewSessionId(), Guid.NewGuid(), Guid.NewGuid(), 3, null, true, true, false);

        Assert.Throws<ArgumentOutOfRangeException>(() => rating.Update(10, null, true, true, false));
    }
}

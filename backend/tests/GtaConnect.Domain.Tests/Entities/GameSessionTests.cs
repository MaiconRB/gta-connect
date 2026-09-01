using GtaConnect.Domain.Entities;

namespace GtaConnect.Domain.Tests.Entities;

public class GameSessionTests
{
    [Fact]
    public void Create_ComDadosValidos_CriaSessao()
    {
        var connectionId = Guid.NewGuid();
        var loggedByProfileId = Guid.NewGuid();

        var session = GameSession.Create(connectionId, loggedByProfileId);

        Assert.Equal(connectionId, session.ConnectionId);
        Assert.Equal(loggedByProfileId, session.LoggedByProfileId);
        Assert.True(session.PlayedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_ComPlayedAtUtcInformado_UsaODataInformada()
    {
        var playedAt = DateTime.UtcNow.AddDays(-2);

        var session = GameSession.Create(Guid.NewGuid(), Guid.NewGuid(), playedAt);

        Assert.Equal(playedAt, session.PlayedAtUtc);
    }

    [Fact]
    public void Create_ComConnectionIdVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => GameSession.Create(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Create_ComDataNoFuturo_LancaArgumentException()
    {
        var futureDate = DateTime.UtcNow.AddDays(1);

        Assert.Throws<ArgumentException>(() => GameSession.Create(Guid.NewGuid(), Guid.NewGuid(), futureDate));
    }
}

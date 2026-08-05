using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Domain.Tests.Entities;

public class PlayerProfileTests
{
    [Fact]
    public void Create_ComDadosValidos_CriaPerfilComValoresEsperados()
    {
        var userId = Guid.NewGuid();

        var profile = PlayerProfile.Create(userId, "  JogadorPS5  ", Platform.Ps5, GameTitle.GtaV);

        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(userId, profile.ApplicationUserId);
        Assert.Equal("JogadorPS5", profile.DisplayName); // Trim aplicado
        Assert.Equal(Platform.Ps5, profile.Platform);
        Assert.Equal(GameTitle.GtaV, profile.GameTitle);
    }

    [Fact]
    public void Create_ComApplicationUserIdVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PlayerProfile.Create(Guid.Empty, "JogadorPS5", Platform.Ps5, GameTitle.GtaV));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ComDisplayNameVazioOuEmBranco_LancaArgumentException(string displayName)
    {
        Assert.Throws<ArgumentException>(() => PlayerProfile.Create(Guid.NewGuid(), displayName, Platform.Ps5, GameTitle.GtaV));
    }
}

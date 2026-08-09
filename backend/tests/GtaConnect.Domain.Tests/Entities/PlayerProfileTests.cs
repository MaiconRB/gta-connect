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

    private static PlayerProfile CreateValidProfile() =>
        PlayerProfile.Create(Guid.NewGuid(), "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

    [Fact]
    public void UpdateProfile_ComDadosValidos_AtualizaCamposEsperados()
    {
        var profile = CreateValidProfile();
        var tags = PlaystyleTag.Corrida | PlaystyleTag.RolePlay;

        profile.UpdateProfile("  Gosto de correr e fazer RP.  ", tags, 120, "  Cayo Perico, corridas oficiais  ");

        Assert.Equal("Gosto de correr e fazer RP.", profile.Bio);
        Assert.Equal(tags, profile.PlaystyleTags);
        Assert.Equal(120, profile.HoursPlayed);
        Assert.Equal("Cayo Perico, corridas oficiais", profile.FavoriteModes);
    }

    [Fact]
    public void UpdateProfile_ComBioEFavoriteModesVazios_ZeraParaNull()
    {
        var profile = CreateValidProfile();

        profile.UpdateProfile("   ", PlaystyleTag.None, 0, "   ");

        Assert.Null(profile.Bio);
        Assert.Null(profile.FavoriteModes);
    }

    [Fact]
    public void UpdateProfile_ComBioMuitoLonga_LancaArgumentException()
    {
        var profile = CreateValidProfile();
        var bioMuitoLonga = new string('a', 501);

        Assert.Throws<ArgumentException>(() => profile.UpdateProfile(bioMuitoLonga, PlaystyleTag.None, 0, null));
    }

    [Fact]
    public void UpdateProfile_ComFavoriteModesMuitoLongo_LancaArgumentException()
    {
        var profile = CreateValidProfile();
        var favoriteModesMuitoLongo = new string('a', 201);

        Assert.Throws<ArgumentException>(() => profile.UpdateProfile(null, PlaystyleTag.None, 0, favoriteModesMuitoLongo));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100_001)]
    public void UpdateProfile_ComHorasJogadasForaDoIntervalo_LancaArgumentOutOfRangeException(int hoursPlayed)
    {
        var profile = CreateValidProfile();

        Assert.Throws<ArgumentOutOfRangeException>(() => profile.UpdateProfile(null, PlaystyleTag.None, hoursPlayed, null));
    }

    [Fact]
    public void UpdateProfile_ComTagDeEstiloInvalida_LancaArgumentException()
    {
        var profile = CreateValidProfile();
        var tagInvalida = (PlaystyleTag)(1 << 20);

        Assert.Throws<ArgumentException>(() => profile.UpdateProfile(null, tagInvalida, 0, null));
    }

    [Fact]
    public void SetAvatar_ComCaminhoValido_AtualizaAvatarPath()
    {
        var profile = CreateValidProfile();

        profile.SetAvatar("/uploads/avatars/abc.jpg");

        Assert.Equal("/uploads/avatars/abc.jpg", profile.AvatarPath);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetAvatar_ComCaminhoVazioOuEmBranco_LancaArgumentException(string avatarPath)
    {
        var profile = CreateValidProfile();

        Assert.Throws<ArgumentException>(() => profile.SetAvatar(avatarPath));
    }
}

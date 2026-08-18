using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Profile;
using GtaConnect.Application.Tests.Common;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Profile;

public class ProfileServiceTests
{
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<IPhotoStorageService> _photoStorageServiceMock = new();
    private readonly ProfileService _sut;

    public ProfileServiceTests()
    {
        _sut = new ProfileService(
            _playerProfileRepositoryMock.Object,
            _photoStorageServiceMock.Object,
            new UpdateProfileRequestValidator(NoOpStringLocalizer.Create()),
            NoOpStringLocalizer.Create());
    }

    private static PlayerProfile CreateValidProfile(Guid userId) =>
        PlayerProfile.Create(userId, "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

    [Fact]
    public async Task GetMyProfileAsync_ComPerfilExistente_RetornaDto()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.GetMyProfileAsync(userId);

        Assert.Equal(profile.Id, result.Id);
        Assert.Equal(profile.DisplayName, result.DisplayName);
    }

    [Fact]
    public async Task GetMyProfileAsync_ComPerfilInexistente_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetMyProfileAsync(userId));
    }

    [Fact]
    public async Task UpdateMyProfileAsync_ComDadosValidos_AtualizaEPersisteAlteracoes()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var request = new UpdateProfileRequestDto("Nova bio", PlaystyleTag.Corrida, 50, "Corridas oficiais", Region.Sul, AvailabilityTag.Madrugada);

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.UpdateMyProfileAsync(userId, request);

        Assert.Equal("Nova bio", result.Bio);
        Assert.Equal(PlaystyleTag.Corrida, result.PlaystyleTags);
        Assert.Equal(50, result.HoursPlayed);
        Assert.Equal("Corridas oficiais", result.FavoriteModes);
        Assert.Equal(Region.Sul, result.Region);
        Assert.Equal(AvailabilityTag.Madrugada, result.AvailabilityTags);
        _playerProfileRepositoryMock.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_ComDadosInvalidos_LancaValidationAppExceptionSemBuscarPerfil()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateProfileRequestDto(new string('a', 501), PlaystyleTag.None, 0, null, null, AvailabilityTag.None);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.UpdateMyProfileAsync(userId, request));

        _playerProfileRepositoryMock.Verify(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_ComPerfilInexistente_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateProfileRequestDto(null, PlaystyleTag.None, 0, null, null, AvailabilityTag.None);

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateMyProfileAsync(userId, request));
    }

    [Fact]
    public async Task UploadAvatarAsync_ComExtensaoInvalida_LancaValidationAppExceptionSemSalvar()
    {
        var userId = Guid.NewGuid();
        using var content = new MemoryStream([1, 2, 3]);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.UploadAvatarAsync(userId, content, "avatar.gif", content.Length));

        _photoStorageServiceMock.Verify(s => s.SaveAvatarAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadAvatarAsync_ComArquivoMuitoGrande_LancaValidationAppExceptionSemSalvar()
    {
        var userId = Guid.NewGuid();
        using var content = new MemoryStream([1, 2, 3]);
        const long tooLarge = 3 * 1024 * 1024;

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.UploadAvatarAsync(userId, content, "avatar.jpg", tooLarge));

        _photoStorageServiceMock.Verify(s => s.SaveAvatarAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadAvatarAsync_ComArquivoValido_SalvaEAtualizaPerfil()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        using var content = new MemoryStream([1, 2, 3]);

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        _photoStorageServiceMock
            .Setup(s => s.SaveAvatarAsync(content, "avatar.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/avatars/abc.jpg");

        var result = await _sut.UploadAvatarAsync(userId, content, "avatar.jpg", content.Length);

        Assert.Equal("/uploads/avatars/abc.jpg", result.AvatarPath);
        _playerProfileRepositoryMock.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
    }
}

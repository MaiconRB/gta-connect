using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.PlayerSearch;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.PlayerSearch;

public class PlayerSearchServiceTests
{
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly PlayerSearchService _sut;

    public PlayerSearchServiceTests()
    {
        _sut = new PlayerSearchService(_playerProfileRepositoryMock.Object);
    }

    private static PlayerProfile CreateValidProfile(Guid userId) =>
        PlayerProfile.Create(userId, "JogadorPS5", Platform.Ps5, GameTitle.GtaV);

    private static PlayerSearchFilterDto CreateFilter(int page = 1, int pageSize = 20) =>
        new(null, PlaystyleTag.None, null, AvailabilityTag.None, page, pageSize);

    [Fact]
    public async Task SearchAsync_ComUsuarioLogadoComPerfil_ExcluiOProprioPerfilDaBusca()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var otherProfile = CreateValidProfile(Guid.NewGuid());

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(myProfile);

        _playerProfileRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<PlayerSearchFilterDto>(), myProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PlayerProfile> { otherProfile }, 1));

        var result = await _sut.SearchAsync(userId, CreateFilter());

        Assert.Single(result.Items);
        Assert.Equal(otherProfile.Id, result.Items[0].Id);
        _playerProfileRepositoryMock.Verify(r => r.SearchAsync(It.IsAny<PlayerSearchFilterDto>(), myProfile.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ComUsuarioSemPerfil_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.SearchAsync(userId, CreateFilter()));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public async Task SearchAsync_ClampaPage(int inputPage, int expectedPage)
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(myProfile);

        _playerProfileRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<PlayerSearchFilterDto>(), myProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PlayerProfile>(), 0));

        var result = await _sut.SearchAsync(userId, CreateFilter(page: inputPage));

        Assert.Equal(expectedPage, result.Page);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(500, 50)]
    [InlineData(10, 10)]
    public async Task SearchAsync_ClampaPageSize(int inputPageSize, int expectedPageSize)
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);

        _playerProfileRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(myProfile);

        _playerProfileRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<PlayerSearchFilterDto>(), myProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PlayerProfile>(), 0));

        var result = await _sut.SearchAsync(userId, CreateFilter(pageSize: inputPageSize));

        Assert.Equal(expectedPageSize, result.PageSize);
    }

    [Fact]
    public async Task GetPlayerProfileAsync_ComPerfilExistente_RetornaDto()
    {
        var profile = CreateValidProfile(Guid.NewGuid());

        _playerProfileRepositoryMock
            .Setup(r => r.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.GetPlayerProfileAsync(profile.Id);

        Assert.Equal(profile.Id, result.Id);
        Assert.Equal(profile.DisplayName, result.DisplayName);
    }

    [Fact]
    public async Task GetPlayerProfileAsync_ComPerfilInexistente_LancaNotFoundException()
    {
        var profileId = Guid.NewGuid();

        _playerProfileRepositoryMock
            .Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetPlayerProfileAsync(profileId));
    }
}

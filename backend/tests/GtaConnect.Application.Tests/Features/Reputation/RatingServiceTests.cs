using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Application.Features.Reputation;
using GtaConnect.Application.Tests.Common;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Reputation;

public class RatingServiceTests
{
    private readonly Mock<IRatingRepository> _ratingRepositoryMock = new();
    private readonly Mock<IConnectionRepository> _connectionRepositoryMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<IBlockRepository> _blockRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly RatingService _sut;

    public RatingServiceTests()
    {
        _blockRepositoryMock
            .Setup(r => r.ExistsEitherDirectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new RatingService(
            _ratingRepositoryMock.Object,
            _connectionRepositoryMock.Object,
            _playerProfileRepositoryMock.Object,
            _blockRepositoryMock.Object,
            _notificationServiceMock.Object,
            NoOpStringLocalizer.Create());
    }

    private static PlayerProfile CreateValidProfile(Guid userId, string displayName = "Jogador") =>
        PlayerProfile.Create(userId, displayName, Platform.Ps5, GameTitle.GtaV);

    private static Connection CreateAcceptedConnection(Guid profileAId, Guid profileBId)
    {
        var connection = Connection.Create(profileAId, profileBId);
        connection.Accept(profileBId);
        return connection;
    }

    [Fact]
    public async Task RateAsync_SemConexaoAceita_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _connectionRepositoryMock
            .Setup(r => r.FindBetweenAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Connection?)null);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.RateAsync(userId, targetProfile.Id, 5, null));
    }

    [Fact]
    public async Task RateAsync_ComConexaoPendente_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");
        var pendingConnection = Connection.Create(myProfile.Id, targetProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _connectionRepositoryMock
            .Setup(r => r.FindBetweenAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingConnection);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.RateAsync(userId, targetProfile.Id, 5, null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task RateAsync_ComScoreForaDoIntervalo_LancaValidationAppException(int invalidScore)
    {
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.RateAsync(userId, Guid.NewGuid(), invalidScore, null));
        _playerProfileRepositoryMock.Verify(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RateAsync_ComConexaoAceitaESemAvaliacaoExistente_CriaNovaAvaliacao()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");
        var connection = CreateAcceptedConnection(myProfile.Id, targetProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _connectionRepositoryMock
            .Setup(r => r.FindBetweenAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);
        _ratingRepositoryMock
            .Setup(r => r.FindAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rating?)null);

        await _sut.RateAsync(userId, targetProfile.Id, 5, "Muito bom!");

        _ratingRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Rating>(rt => rt.Score == 5 && rt.Comment == "Muito bom!"), It.IsAny<CancellationToken>()),
            Times.Once);
        _notificationServiceMock.Verify(
            n => n.NotifyAsync(targetProfile.Id, myProfile.Id, NotificationType.RatingReceived, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RateAsync_ComAvaliacaoJaExistente_AtualizaEmVezDeDuplicar()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");
        var connection = CreateAcceptedConnection(myProfile.Id, targetProfile.Id);
        var existingRating = Rating.Create(myProfile.Id, targetProfile.Id, 3, "ok");

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _connectionRepositoryMock
            .Setup(r => r.FindBetweenAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(connection);
        _ratingRepositoryMock
            .Setup(r => r.FindAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRating);

        await _sut.RateAsync(userId, targetProfile.Id, 5, "Mudei de ideia, foi ótimo!");

        Assert.Equal(5, existingRating.Score);
        _ratingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Rating>(), It.IsAny<CancellationToken>()), Times.Never);
        _ratingRepositoryMock.Verify(r => r.UpdateAsync(existingRating, It.IsAny<CancellationToken>()), Times.Once);
    }
}

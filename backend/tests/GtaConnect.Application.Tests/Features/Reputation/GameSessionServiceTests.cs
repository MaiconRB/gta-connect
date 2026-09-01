using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Application.Features.Reputation;
using GtaConnect.Application.Tests.Common;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Reputation;

public class GameSessionServiceTests
{
    private readonly Mock<IGameSessionRepository> _gameSessionRepositoryMock = new();
    private readonly Mock<IConnectionRepository> _connectionRepositoryMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly GameSessionService _sut;

    public GameSessionServiceTests()
    {
        _sut = new GameSessionService(
            _gameSessionRepositoryMock.Object,
            _connectionRepositoryMock.Object,
            _playerProfileRepositoryMock.Object,
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
    public async Task LogSessionAsync_ComConexaoAceitaEuParticipando_CriaSessaoENotificaOOutro()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var otherProfile = CreateValidProfile(Guid.NewGuid(), "Parceiro");
        var connection = CreateAcceptedConnection(myProfile.Id, otherProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _connectionRepositoryMock.Setup(r => r.GetByIdAsync(connection.Id, It.IsAny<CancellationToken>())).ReturnsAsync(connection);

        var sessionId = await _sut.LogSessionAsync(userId, connection.Id);

        Assert.NotEqual(Guid.Empty, sessionId);
        _gameSessionRepositoryMock.Verify(
            r => r.AddAsync(It.Is<GameSession>(s => s.ConnectionId == connection.Id && s.LoggedByProfileId == myProfile.Id), It.IsAny<CancellationToken>()),
            Times.Once);
        _notificationServiceMock.Verify(
            n => n.NotifyAsync(otherProfile.Id, myProfile.Id, NotificationType.SessionLogged, sessionId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogSessionAsync_ComConexaoPendente_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var otherProfile = CreateValidProfile(Guid.NewGuid(), "Parceiro");
        var connection = Connection.Create(myProfile.Id, otherProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _connectionRepositoryMock.Setup(r => r.GetByIdAsync(connection.Id, It.IsAny<CancellationToken>())).ReturnsAsync(connection);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.LogSessionAsync(userId, connection.Id));
        _gameSessionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogSessionAsync_ComConexaoQueNaoParticipo_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var connection = CreateAcceptedConnection(Guid.NewGuid(), Guid.NewGuid());

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _connectionRepositoryMock.Setup(r => r.GetByIdAsync(connection.Id, It.IsAny<CancellationToken>())).ReturnsAsync(connection);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.LogSessionAsync(userId, connection.Id));
    }

    [Fact]
    public async Task LogSessionAsync_ComConexaoInexistente_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var connectionId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _connectionRepositoryMock.Setup(r => r.GetByIdAsync(connectionId, It.IsAny<CancellationToken>())).ReturnsAsync((Connection?)null);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.LogSessionAsync(userId, connectionId));
    }
}

using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Application.Features.Reputation;
using GtaConnect.Application.Tests.Common;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Reputation;

public class ConnectionServiceTests
{
    private readonly Mock<IConnectionRepository> _connectionRepositoryMock = new();
    private readonly Mock<IRatingRepository> _ratingRepositoryMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<IBlockRepository> _blockRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly ConnectionService _sut;

    public ConnectionServiceTests()
    {
        _blockRepositoryMock
            .Setup(r => r.ExistsEitherDirectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ratingRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rating?)null);

        _sut = new ConnectionService(
            _connectionRepositoryMock.Object,
            _ratingRepositoryMock.Object,
            _playerProfileRepositoryMock.Object,
            _blockRepositoryMock.Object,
            _notificationServiceMock.Object,
            NoOpStringLocalizer.Create());
    }

    private static PlayerProfile CreateValidProfile(Guid userId, string displayName = "Jogador") =>
        PlayerProfile.Create(userId, displayName, Platform.Ps5, GameTitle.GtaV);

    [Fact]
    public async Task SendRequestAsync_SemConexaoExistente_CriaNovaComStatusPending()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId, "Remetente");
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _connectionRepositoryMock
            .Setup(r => r.FindBetweenAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Connection?)null);

        var result = await _sut.SendRequestAsync(userId, targetProfile.Id);

        Assert.Equal(ConnectionStatus.Pending, result.Status);
        Assert.True(result.IsRequester);
        _connectionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Connection>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(
            n => n.NotifyAsync(targetProfile.Id, myProfile.Id, NotificationType.ConnectionRequestReceived, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendRequestAsync_ComConexaoPendenteExistente_NaoDuplicaEDevolveAExistente()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId, "Remetente");
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");
        var existingConnection = Connection.Create(myProfile.Id, targetProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _connectionRepositoryMock
            .Setup(r => r.FindBetweenAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConnection);

        var result = await _sut.SendRequestAsync(userId, targetProfile.Id);

        Assert.Equal(existingConnection.Id, result.ConnectionId);
        _connectionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Connection>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationServiceMock.Verify(
            n => n.NotifyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<NotificationType>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SendRequestAsync_ComConexaoDeclinadaExistente_CriaUmaNova()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId, "Remetente");
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");
        var declinedConnection = Connection.Create(myProfile.Id, targetProfile.Id);
        declinedConnection.Decline(myProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _connectionRepositoryMock
            .Setup(r => r.FindBetweenAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(declinedConnection);

        await _sut.SendRequestAsync(userId, targetProfile.Id);

        _connectionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Connection>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendRequestAsync_ComBloqueioEntreOsDois_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var targetProfile = CreateValidProfile(Guid.NewGuid(), "Alvo");

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(targetProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(targetProfile);
        _blockRepositoryMock
            .Setup(r => r.ExistsEitherDirectionAsync(myProfile.Id, targetProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.SendRequestAsync(userId, targetProfile.Id));
    }

    [Fact]
    public async Task AcceptAsync_ChamadoPeloAddressee_Aceita()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var connection = Connection.Create(Guid.NewGuid(), myProfile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _connectionRepositoryMock.Setup(r => r.GetByIdAsync(connection.Id, It.IsAny<CancellationToken>())).ReturnsAsync(connection);

        await _sut.AcceptAsync(userId, connection.Id);

        Assert.Equal(ConnectionStatus.Accepted, connection.Status);
        _connectionRepositoryMock.Verify(r => r.UpdateStatusAsync(connection, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_ComConexaoQueNaoParticipa_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var connectionOfOtherPeople = Connection.Create(Guid.NewGuid(), Guid.NewGuid());

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _connectionRepositoryMock.Setup(r => r.GetByIdAsync(connectionOfOtherPeople.Id, It.IsAny<CancellationToken>())).ReturnsAsync(connectionOfOtherPeople);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.AcceptAsync(userId, connectionOfOtherPeople.Id));
    }

    [Fact]
    public async Task DeclineAsync_ChamadoPeloRequester_Cancela()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var connection = Connection.Create(myProfile.Id, Guid.NewGuid());

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _connectionRepositoryMock.Setup(r => r.GetByIdAsync(connection.Id, It.IsAny<CancellationToken>())).ReturnsAsync(connection);

        await _sut.DeclineAsync(userId, connection.Id);

        Assert.Equal(ConnectionStatus.Declined, connection.Status);
    }
}

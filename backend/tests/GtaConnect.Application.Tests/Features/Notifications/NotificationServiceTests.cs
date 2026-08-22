using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Notifications;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock = new();
    private readonly Mock<INotificationPusher> _notificationPusherMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(
            _notificationRepositoryMock.Object,
            _notificationPusherMock.Object,
            _playerProfileRepositoryMock.Object);
    }

    private static PlayerProfile CreateValidProfile(Guid userId, string displayName = "Jogador") =>
        PlayerProfile.Create(userId, displayName, Platform.Ps5, GameTitle.GtaV);

    [Fact]
    public async Task NotifyAsync_ComDestinatarioDiferenteDoAtor_CriaPersisteEEmpurra()
    {
        var recipientProfile = CreateValidProfile(Guid.NewGuid(), "Destinatário");
        var actorProfile = CreateValidProfile(Guid.NewGuid(), "Autor");
        var relatedEntityId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(recipientProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(recipientProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(actorProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(actorProfile);

        await _sut.NotifyAsync(recipientProfile.Id, actorProfile.Id, NotificationType.MessageReceived, relatedEntityId);

        _notificationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationPusherMock.Verify(
            p => p.PushAsync(
                recipientProfile.ApplicationUserId,
                It.Is<NotificationDto>(dto => dto.ActorDisplayName == "Autor" && dto.Type == NotificationType.MessageReceived && dto.RelatedEntityId == relatedEntityId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_QuandoDestinatarioEAtorSaoOMesmo_NaoCriaNadaNemEmpurra()
    {
        var profile = CreateValidProfile(Guid.NewGuid());

        await _sut.NotifyAsync(profile.Id, profile.Id, NotificationType.PostLiked, Guid.NewGuid());

        _playerProfileRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationPusherMock.Verify(p => p.PushAsync(It.IsAny<Guid>(), It.IsAny<NotificationDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifyAsync_QuandoPushFalha_NaoLancaExcecaoEJaPersistiu()
    {
        var recipientProfile = CreateValidProfile(Guid.NewGuid());
        var actorProfile = CreateValidProfile(Guid.NewGuid());

        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(recipientProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(recipientProfile);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(actorProfile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(actorProfile);
        _notificationPusherMock
            .Setup(p => p.PushAsync(It.IsAny<Guid>(), It.IsAny<NotificationDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Hub indisponível"));

        await _sut.NotifyAsync(recipientProfile.Id, actorProfile.Id, NotificationType.RatingReceived, null);

        _notificationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_ComNotificacaoAlheia_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var notification = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), NotificationType.PostLiked, null);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _notificationRepositoryMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.MarkAsReadAsync(userId, notification.Id));
    }

    [Fact]
    public async Task MarkAsReadAsync_ComNotificacaoPropria_MarcaComoLida()
    {
        var userId = Guid.NewGuid();
        var myProfile = CreateValidProfile(userId);
        var notification = Notification.Create(myProfile.Id, Guid.NewGuid(), NotificationType.PostLiked, null);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(myProfile);
        _notificationRepositoryMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

        await _sut.MarkAsReadAsync(userId, notification.Id);

        Assert.True(notification.IsRead);
        _notificationRepositoryMock.Verify(r => r.MarkAsReadAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
    }
}

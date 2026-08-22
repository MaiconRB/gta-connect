using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Notifications;

public class NotificationService : INotificationService
{
    private const int RecentNotificationsLimit = 30;

    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPusher _notificationPusher;
    private readonly IPlayerProfileRepository _playerProfileRepository;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationPusher notificationPusher,
        IPlayerProfileRepository playerProfileRepository)
    {
        _notificationRepository = notificationRepository;
        _notificationPusher = notificationPusher;
        _playerProfileRepository = playerProfileRepository;
    }

    public async Task NotifyAsync(Guid recipientProfileId, Guid actorProfileId, NotificationType type, Guid? relatedEntityId, CancellationToken cancellationToken = default)
    {
        if (recipientProfileId == actorProfileId)
        {
            return;
        }

        var recipientProfile = await _playerProfileRepository.GetByIdAsync(recipientProfileId, cancellationToken);
        var actorProfile = await _playerProfileRepository.GetByIdAsync(actorProfileId, cancellationToken);
        if (recipientProfile is null || actorProfile is null)
        {
            return;
        }

        var notification = Notification.Create(recipientProfileId, actorProfileId, type, relatedEntityId);
        await _notificationRepository.AddAsync(notification, cancellationToken);

        var dto = ToDto(notification, actorProfile);

        try
        {
            await _notificationPusher.PushAsync(recipientProfile.ApplicationUserId, dto, cancellationToken);
        }
        catch
        {
            // Push é melhor-esforço — falha aqui não pode derrubar a ação que gerou a notificação.
        }
    }

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        return await _notificationRepository.GetRecentForProfileAsync(myProfile.Id, RecentNotificationsLimit, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        return await _notificationRepository.GetUnreadCountAsync(myProfile.Id, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        // "Não existe" cobre tanto notificação inexistente quanto alheia — mesmo padrão
        // já usado em conversas/conexões.
        if (notification is null || notification.RecipientProfileId != myProfile.Id)
        {
            throw new NotFoundException(nameof(Notification), notificationId);
        }

        notification.MarkAsRead();
        await _notificationRepository.MarkAsReadAsync(notification, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var myProfile = await GetProfileOrThrowAsync(userId, cancellationToken);
        await _notificationRepository.MarkAllAsReadAsync(myProfile.Id, cancellationToken);
    }

    private async Task<PlayerProfile> GetProfileOrThrowAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);
    }

    private static NotificationDto ToDto(Notification notification, PlayerProfile actorProfile) => new(
        notification.Id,
        notification.Type,
        actorProfile.Id,
        actorProfile.DisplayName,
        actorProfile.AvatarPath,
        notification.RelatedEntityId,
        notification.IsRead,
        notification.CreatedAtUtc);
}

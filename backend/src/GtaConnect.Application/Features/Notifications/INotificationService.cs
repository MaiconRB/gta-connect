using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Notifications;

public interface INotificationService
{
    /// <summary>Cria, persiste e empurra em tempo real uma notificação. Não faz nada se recipient == actor.</summary>
    Task NotifyAsync(Guid recipientProfileId, Guid actorProfileId, NotificationType type, Guid? relatedEntityId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

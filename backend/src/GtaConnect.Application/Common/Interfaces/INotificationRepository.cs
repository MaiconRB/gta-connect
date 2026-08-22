using GtaConnect.Application.Features.Notifications;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>As N notificações mais recentes do destinatário, já com o autor resolvido.</summary>
    Task<IReadOnlyList<NotificationDto>> GetRecentForProfileAsync(Guid profileId, int limit, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>Marca todas as notificações não lidas do perfil como lidas, em lote (sem carregar entidades pra memória).</summary>
    Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default);
}

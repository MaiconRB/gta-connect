using GtaConnect.Application.Features.Notifications;

namespace GtaConnect.Application.Common.Interfaces;

// Abstrai a entrega em tempo real pra Application. A Application não conhece SignalR/
// IHubContext — só sabe que existe algo que sabe empurrar uma notificação pro usuário.
// Implementação concreta mora na Api (não na Infrastructure), porque é a única camada
// que conhece o ChatHub.
public interface INotificationPusher
{
    /// <param name="recipientUserId">ApplicationUserId do destinatário — não o ProfileId (é o que o Hub usa pra rotear via Clients.User).</param>
    Task PushAsync(Guid recipientUserId, NotificationDto notification, CancellationToken cancellationToken = default);
}

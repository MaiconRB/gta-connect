using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace GtaConnect.Api.Hubs;

// Implementação concreta de INotificationPusher (interface da Application) — mora na Api,
// não na Infrastructure, porque é a única camada que conhece ChatHub/IHubContext.
public class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRNotificationPusher(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PushAsync(Guid recipientUserId, NotificationDto notification, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.User(recipientUserId.ToString()).SendAsync("ReceiveNotification", notification, cancellationToken);
    }
}

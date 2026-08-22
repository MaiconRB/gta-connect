using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications.SingleOrDefaultAsync(n => n.Id == notificationId, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetRecentForProfileAsync(Guid profileId, int limit, CancellationToken cancellationToken = default)
    {
        var query =
            from n in _dbContext.Notifications
            where n.RecipientProfileId == profileId
            join actor in _dbContext.PlayerProfiles on n.ActorProfileId equals actor.Id
            orderby n.CreatedAtUtc descending
            select new NotificationDto(n.Id, n.Type, actor.Id, actor.DisplayName, actor.AvatarPath, n.RelatedEntityId, n.IsRead, n.CreatedAtUtc);

        return await query.Take(limit).ToListAsync(cancellationToken);
    }

    public Task<int> GetUnreadCountAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications.CountAsync(n => n.RecipientProfileId == profileId && !n.IsRead, cancellationToken);
    }

    public async Task MarkAsReadAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        // "notification" já vem rastreado (mesmo DbContext/escopo) — nada de .Update() aqui,
        // mesmo motivo já documentado em ChatRepository/ConnectionRepository.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task MarkAllAsReadAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return _dbContext.Notifications
            .Where(n => n.RecipientProfileId == profileId && !n.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAtUtc, now),
                cancellationToken);
    }
}

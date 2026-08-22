using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Notifications;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    Guid ActorProfileId,
    string ActorDisplayName,
    string? ActorAvatarPath,
    Guid? RelatedEntityId,
    bool IsRead,
    DateTime CreatedAtUtc);

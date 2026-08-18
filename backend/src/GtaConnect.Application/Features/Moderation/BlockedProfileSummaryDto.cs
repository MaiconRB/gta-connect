namespace GtaConnect.Application.Features.Moderation;

public record BlockedProfileSummaryDto(Guid BlockedProfileId, string DisplayName, string? AvatarPath, DateTime BlockedAtUtc);

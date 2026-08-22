namespace GtaConnect.Application.Features.ModerationReview;

public record ReportedProfileSummaryDto(
    Guid ProfileId,
    string DisplayName,
    string? AvatarPath,
    int PendingCount,
    int TotalCount,
    bool IsBanned);

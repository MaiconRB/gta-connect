using GtaConnect.Application.Features.Feed;

namespace GtaConnect.Application.Features.ModerationReview;

public record ReportedProfileDetailDto(
    Guid ProfileId,
    string DisplayName,
    string? AvatarPath,
    bool IsBanned,
    IReadOnlyList<ReportDetailDto> Reports,
    IReadOnlyList<PostSummaryDto> RecentPosts);

namespace GtaConnect.Application.Features.Feed;

public record PostSummaryDto(
    Guid Id,
    Guid AuthorProfileId,
    string AuthorDisplayName,
    string? AuthorAvatarPath,
    string? Content,
    string? PhotoPath,
    DateTime CreatedAtUtc,
    int LikeCount,
    bool LikedByMe);

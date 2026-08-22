namespace GtaConnect.Application.Features.ModerationReview;

public interface IModerationReviewService
{
    Task<IReadOnlyList<ReportedProfileSummaryDto>> GetReportedProfilesAsync(CancellationToken cancellationToken = default);

    Task<ReportedProfileDetailDto> GetReportedProfileDetailAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task MarkReportReviewedAsync(Guid userId, Guid reportId, CancellationToken cancellationToken = default);

    Task BanProfileAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task UnbanProfileAsync(Guid profileId, CancellationToken cancellationToken = default);

    Task DeletePostAsModeratorAsync(Guid postId, CancellationToken cancellationToken = default);
}

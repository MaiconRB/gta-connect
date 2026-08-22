using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Feed;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Features.ModerationReview;

public class ModerationReviewService : IModerationReviewService
{
    private const int RecentPostsLimit = 30;

    private readonly IReportRepository _reportRepository;
    private readonly IFeedRepository _feedRepository;
    private readonly IFeedService _feedService;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IIdentityService _identityService;

    public ModerationReviewService(
        IReportRepository reportRepository,
        IFeedRepository feedRepository,
        IFeedService feedService,
        IPlayerProfileRepository playerProfileRepository,
        IIdentityService identityService)
    {
        _reportRepository = reportRepository;
        _feedRepository = feedRepository;
        _feedService = feedService;
        _playerProfileRepository = playerProfileRepository;
        _identityService = identityService;
    }

    public Task<IReadOnlyList<ReportedProfileSummaryDto>> GetReportedProfilesAsync(CancellationToken cancellationToken = default)
    {
        return _reportRepository.GetGroupedByReportedProfileAsync(cancellationToken);
    }

    public async Task<ReportedProfileDetailDto> GetReportedProfileDetailAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var profile = await _playerProfileRepository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), profileId);

        var reports = await _reportRepository.GetByReportedProfileAsync(profileId, cancellationToken);
        var recentPosts = await _feedRepository.GetByAuthorAsync(profileId, RecentPostsLimit, cancellationToken);
        var isBanned = await _identityService.IsUserBannedAsync(profile.ApplicationUserId);

        return new ReportedProfileDetailDto(profile.Id, profile.DisplayName, profile.AvatarPath, isBanned, reports, recentPosts);
    }

    public async Task MarkReportReviewedAsync(Guid userId, Guid reportId, CancellationToken cancellationToken = default)
    {
        var moderatorProfile = await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);

        var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), reportId);

        report.MarkReviewed(moderatorProfile.Id);
        await _reportRepository.UpdateAsync(report, cancellationToken);
    }

    public async Task BanProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var profile = await _playerProfileRepository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), profileId);

        await _identityService.BanUserAsync(profile.ApplicationUserId);
    }

    public async Task UnbanProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var profile = await _playerProfileRepository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), profileId);

        await _identityService.UnbanUserAsync(profile.ApplicationUserId);
    }

    public Task DeletePostAsModeratorAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return _feedService.DeletePostAsModeratorAsync(postId, cancellationToken);
    }
}

using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Feed;
using GtaConnect.Application.Features.ModerationReview;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.ModerationReview;

public class ModerationReviewServiceTests
{
    private readonly Mock<IReportRepository> _reportRepositoryMock = new();
    private readonly Mock<IFeedRepository> _feedRepositoryMock = new();
    private readonly Mock<IFeedService> _feedServiceMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly ModerationReviewService _sut;

    public ModerationReviewServiceTests()
    {
        _sut = new ModerationReviewService(
            _reportRepositoryMock.Object,
            _feedRepositoryMock.Object,
            _feedServiceMock.Object,
            _playerProfileRepositoryMock.Object,
            _identityServiceMock.Object);
    }

    private static PlayerProfile CreateValidProfile(Guid userId, string displayName = "Jogador") =>
        PlayerProfile.Create(userId, displayName, Platform.Ps5, GameTitle.GtaV);

    [Fact]
    public async Task GetReportedProfilesAsync_PassaAdianteOQueORepositorioDevolve()
    {
        var summaries = new List<ReportedProfileSummaryDto>
        {
            new(Guid.NewGuid(), "Jogador1", null, 2, 3, false),
        };
        _reportRepositoryMock.Setup(r => r.GetGroupedByReportedProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(summaries);

        var result = await _sut.GetReportedProfilesAsync();

        Assert.Same(summaries, result);
    }

    [Fact]
    public async Task GetReportedProfileDetailAsync_ComPerfilInexistente_LancaNotFoundException()
    {
        var profileId = Guid.NewGuid();
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>())).ReturnsAsync((PlayerProfile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetReportedProfileDetailAsync(profileId));
    }

    [Fact]
    public async Task GetReportedProfileDetailAsync_ComPerfilExistente_MontaDetalheCompleto()
    {
        var applicationUserId = Guid.NewGuid();
        var profile = CreateValidProfile(applicationUserId, "Denunciado");
        var reports = new List<ReportDetailDto> { new(Guid.NewGuid(), "Denunciante", ReportReason.Spam, null, ReportStatus.Pending, DateTime.UtcNow, null) };
        var posts = new List<PostSummaryDto> { new(Guid.NewGuid(), profile.Id, profile.DisplayName, null, "post", null, DateTime.UtcNow, 0, false) };

        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _reportRepositoryMock.Setup(r => r.GetByReportedProfileAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reports);
        _feedRepositoryMock.Setup(r => r.GetByAuthorAsync(profile.Id, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(posts);
        _identityServiceMock.Setup(s => s.IsUserBannedAsync(applicationUserId)).ReturnsAsync(true);

        var result = await _sut.GetReportedProfileDetailAsync(profile.Id);

        Assert.Equal(profile.Id, result.ProfileId);
        Assert.True(result.IsBanned);
        Assert.Same(reports, result.Reports);
        Assert.Same(posts, result.RecentPosts);
    }

    [Fact]
    public async Task MarkReportReviewedAsync_ComReportInexistente_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var moderatorProfile = CreateValidProfile(userId);
        var reportId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(moderatorProfile);
        _reportRepositoryMock.Setup(r => r.GetByIdAsync(reportId, It.IsAny<CancellationToken>())).ReturnsAsync((Report?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.MarkReportReviewedAsync(userId, reportId));
    }

    [Fact]
    public async Task MarkReportReviewedAsync_ComReportExistente_MarcaRevisadoEPersiste()
    {
        var userId = Guid.NewGuid();
        var moderatorProfile = CreateValidProfile(userId);
        var report = Report.Create(Guid.NewGuid(), Guid.NewGuid(), ReportReason.Spam, null);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(moderatorProfile);
        _reportRepositoryMock.Setup(r => r.GetByIdAsync(report.Id, It.IsAny<CancellationToken>())).ReturnsAsync(report);

        await _sut.MarkReportReviewedAsync(userId, report.Id);

        Assert.Equal(ReportStatus.Reviewed, report.Status);
        Assert.Equal(moderatorProfile.Id, report.ReviewedByProfileId);
        _reportRepositoryMock.Verify(r => r.UpdateAsync(report, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BanProfileAsync_ResolvePerfilEDelegaPraIdentityService()
    {
        var applicationUserId = Guid.NewGuid();
        var profile = CreateValidProfile(applicationUserId);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        await _sut.BanProfileAsync(profile.Id);

        _identityServiceMock.Verify(s => s.BanUserAsync(applicationUserId), Times.Once);
    }

    [Fact]
    public async Task UnbanProfileAsync_ResolvePerfilEDelegaPraIdentityService()
    {
        var applicationUserId = Guid.NewGuid();
        var profile = CreateValidProfile(applicationUserId);
        _playerProfileRepositoryMock.Setup(r => r.GetByIdAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        await _sut.UnbanProfileAsync(profile.Id);

        _identityServiceMock.Verify(s => s.UnbanUserAsync(applicationUserId), Times.Once);
    }

    [Fact]
    public async Task DeletePostAsModeratorAsync_DelegaPraFeedService()
    {
        var postId = Guid.NewGuid();

        await _sut.DeletePostAsModeratorAsync(postId);

        _feedServiceMock.Verify(s => s.DeletePostAsModeratorAsync(postId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

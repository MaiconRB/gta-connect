using System.Text;
using GtaConnect.Application.Common;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Feed;
using GtaConnect.Application.Features.Notifications;
using GtaConnect.Application.Features.Reputation;
using GtaConnect.Application.Tests.Common;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Moq;

namespace GtaConnect.Application.Tests.Features.Feed;

public class FeedServiceTests
{
    private readonly Mock<IFeedRepository> _feedRepositoryMock = new();
    private readonly Mock<IPlayerProfileRepository> _playerProfileRepositoryMock = new();
    private readonly Mock<IPhotoStorageService> _photoStorageServiceMock = new();
    private readonly Mock<IBlockRepository> _blockRepositoryMock = new();
    private readonly Mock<IConnectionRepository> _connectionRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly FeedService _sut;

    public FeedServiceTests()
    {
        _blockRepositoryMock
            .Setup(r => r.GetBlockedOrBlockingProfileIdsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid>());

        _sut = new FeedService(
            _feedRepositoryMock.Object,
            _playerProfileRepositoryMock.Object,
            _photoStorageServiceMock.Object,
            _blockRepositoryMock.Object,
            _connectionRepositoryMock.Object,
            _notificationServiceMock.Object,
            NoOpStringLocalizer.Create());
    }

    private static PlayerProfile CreateValidProfile(Guid userId, string displayName = "Jogador") =>
        PlayerProfile.Create(userId, displayName, Platform.Ps5, GameTitle.GtaV);

    [Fact]
    public async Task CreatePostAsync_ComSoTexto_CriaPost()
    {
        var userId = Guid.NewGuid();
        var author = CreateValidProfile(userId);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(author);

        var result = await _sut.CreatePostAsync(userId, "Finalmente terminei a campanha!", null, null, 0);

        Assert.Equal(author.Id, result.AuthorProfileId);
        Assert.Equal("Finalmente terminei a campanha!", result.Content);
        Assert.Null(result.PhotoPath);
        Assert.Equal(0, result.LikeCount);
        Assert.False(result.LikedByMe);
        _feedRepositoryMock.Verify(r => r.AddPostAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()), Times.Once);
        _photoStorageServiceMock.Verify(s => s.SavePostPhotoAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePostAsync_ComFoto_SalvaFotoEUsaOCaminhoRetornado()
    {
        var userId = Guid.NewGuid();
        var author = CreateValidProfile(userId);
        using var photoStream = new MemoryStream(Encoding.UTF8.GetBytes("fake-image-bytes"));

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(author);
        _photoStorageServiceMock
            .Setup(s => s.SavePostPhotoAsync(photoStream, "foto.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/posts/abc.jpg");

        var result = await _sut.CreatePostAsync(userId, null, photoStream, "foto.jpg", 1024);

        Assert.Equal("/uploads/posts/abc.jpg", result.PhotoPath);
    }

    [Fact]
    public async Task CreatePostAsync_SemTextoNemFoto_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.CreatePostAsync(userId, null, null, null, 0));
        _playerProfileRepositoryMock.Verify(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePostAsync_ComExtensaoInvalida_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();
        using var photoStream = new MemoryStream();

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.CreatePostAsync(userId, null, photoStream, "arquivo.exe", 1024));
    }

    [Fact]
    public async Task CreatePostAsync_ComFotoMuitoGrande_LancaValidationAppException()
    {
        var userId = Guid.NewGuid();
        using var photoStream = new MemoryStream();

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.CreatePostAsync(userId, null, photoStream, "foto.jpg", 3 * 1024 * 1024));
    }

    [Fact]
    public async Task GetFeedAsync_ExcluiPostsDeQuemEstaBloqueado()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var blockedProfileId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _blockRepositoryMock
            .Setup(r => r.GetBlockedOrBlockingProfileIdsAsync(profile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { blockedProfileId });
        _feedRepositoryMock
            .Setup(r => r.GetFeedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), null, profile.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PostSummaryDto>(), 0));

        await _sut.GetFeedAsync(userId, 1, 20, onlyConnections: false);

        _feedRepositoryMock.Verify(
            r => r.GetFeedAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(blockedProfileId)),
                null,
                profile.Id,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _connectionRepositoryMock.Verify(r => r.GetConnectionsForProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetFeedAsync_ComOnlyConnections_RestringeAosPerfisComConexaoAceita()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var acceptedProfileId = Guid.NewGuid();
        var pendingProfileId = Guid.NewGuid();
        var connections = new List<ConnectionSummaryDto>
        {
            new(Guid.NewGuid(), acceptedProfileId, "Aceito", null, ConnectionStatus.Accepted, true, DateTime.UtcNow),
            new(Guid.NewGuid(), pendingProfileId, "Pendente", null, ConnectionStatus.Pending, true, DateTime.UtcNow),
        };

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _connectionRepositoryMock.Setup(r => r.GetConnectionsForProfileAsync(profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(connections);
        _feedRepositoryMock
            .Setup(r => r.GetFeedAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<IReadOnlyCollection<Guid>?>(), profile.Id, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PostSummaryDto>(), 0));

        await _sut.GetFeedAsync(userId, 1, 20, onlyConnections: true);

        _feedRepositoryMock.Verify(
            r => r.GetFeedAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.Is<IReadOnlyCollection<Guid>?>(ids => ids != null && ids.Count == 1 && ids.Contains(acceptedProfileId)),
                profile.Id,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeletePostAsync_ComPostAlheio_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var otherPersonsPost = Post.Create(Guid.NewGuid(), "post de outra pessoa", null);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _feedRepositoryMock.Setup(r => r.GetPostByIdAsync(otherPersonsPost.Id, It.IsAny<CancellationToken>())).ReturnsAsync(otherPersonsPost);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeletePostAsync(userId, otherPersonsPost.Id));
        _feedRepositoryMock.Verify(r => r.DeletePostAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeletePostAsync_ComPostProprio_Apaga()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var ownPost = Post.Create(profile.Id, "meu post", null);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _feedRepositoryMock.Setup(r => r.GetPostByIdAsync(ownPost.Id, It.IsAny<CancellationToken>())).ReturnsAsync(ownPost);

        await _sut.DeletePostAsync(userId, ownPost.Id);

        _feedRepositoryMock.Verify(r => r.DeletePostAsync(ownPost, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_SemCurtidaExistente_Curte()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var authorProfileId = Guid.NewGuid();
        var post = Post.Create(authorProfileId, "post pra curtir", null);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _feedRepositoryMock.Setup(r => r.GetPostByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _feedRepositoryMock.Setup(r => r.FindLikeAsync(post.Id, profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync((PostLike?)null);
        _feedRepositoryMock.Setup(r => r.GetLikeCountAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ToggleLikeAsync(userId, post.Id);

        Assert.True(result.Liked);
        Assert.Equal(1, result.LikeCount);
        _feedRepositoryMock.Verify(r => r.AddLikeAsync(It.IsAny<PostLike>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(
            n => n.NotifyAsync(authorProfileId, profile.Id, NotificationType.PostLiked, post.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ToggleLikeAsync_ComCurtidaExistente_Descurte()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var post = Post.Create(Guid.NewGuid(), "post curtido", null);
        var existingLike = PostLike.Create(post.Id, profile.Id);

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _feedRepositoryMock.Setup(r => r.GetPostByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _feedRepositoryMock.Setup(r => r.FindLikeAsync(post.Id, profile.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingLike);
        _feedRepositoryMock.Setup(r => r.GetLikeCountAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _sut.ToggleLikeAsync(userId, post.Id);

        Assert.False(result.Liked);
        Assert.Equal(0, result.LikeCount);
        _feedRepositoryMock.Verify(r => r.RemoveLikeAsync(existingLike, It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(
            n => n.NotifyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<NotificationType>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ToggleLikeAsync_ComPostInexistente_LancaNotFoundException()
    {
        var userId = Guid.NewGuid();
        var profile = CreateValidProfile(userId);
        var postId = Guid.NewGuid();

        _playerProfileRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _feedRepositoryMock.Setup(r => r.GetPostByIdAsync(postId, It.IsAny<CancellationToken>())).ReturnsAsync((Post?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ToggleLikeAsync(userId, postId));
    }
}

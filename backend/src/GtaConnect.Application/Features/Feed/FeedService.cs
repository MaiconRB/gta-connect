using GtaConnect.Application.Common;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Resources;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Application.Features.Feed;

public class FeedService : IFeedService
{
    private static readonly string[] AllowedPhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxPhotoSizeBytes = 2 * 1024 * 1024;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;

    private readonly IFeedRepository _feedRepository;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IPhotoStorageService _photoStorageService;
    private readonly IBlockRepository _blockRepository;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public FeedService(
        IFeedRepository feedRepository,
        IPlayerProfileRepository playerProfileRepository,
        IPhotoStorageService photoStorageService,
        IBlockRepository blockRepository,
        IStringLocalizer<SharedResource> localizer)
    {
        _feedRepository = feedRepository;
        _playerProfileRepository = playerProfileRepository;
        _photoStorageService = photoStorageService;
        _blockRepository = blockRepository;
        _localizer = localizer;
    }

    public async Task<PostSummaryDto> CreatePostAsync(
        Guid userId,
        string? content,
        Stream? photoContent,
        string? photoFileName,
        long photoContentLength,
        CancellationToken cancellationToken = default)
    {
        // Checagem "não pode estar tudo vazio" acontece AQUI, antes de chamar o domínio —
        // Post.Create também valida isso (defesa em profundidade), mas ArgumentException
        // não é mapeada pelo GlobalExceptionHandler (viraria 500), e postar vazio é um erro
        // de usuário genuinamente alcançável, não um "nunca deveria acontecer".
        if (string.IsNullOrWhiteSpace(content) && photoContent is null)
        {
            throw new ValidationAppException("post", _localizer["Feed_PostEmpty"]);
        }

        string? photoPath = null;
        if (photoContent is not null)
        {
            var extension = Path.GetExtension(photoFileName ?? string.Empty).ToLowerInvariant();
            if (!AllowedPhotoExtensions.Contains(extension))
            {
                throw new ValidationAppException("photo", _localizer["Feed_PhotoInvalidType"]);
            }

            if (photoContentLength > MaxPhotoSizeBytes)
            {
                throw new ValidationAppException("photo", _localizer["Feed_PhotoTooLarge"]);
            }

            photoPath = await _photoStorageService.SavePostPhotoAsync(photoContent, photoFileName!, cancellationToken);
        }

        var author = await GetProfileOrThrowAsync(userId, cancellationToken);
        var post = Post.Create(author.Id, content, photoPath);
        await _feedRepository.AddPostAsync(post, cancellationToken);

        return new PostSummaryDto(post.Id, author.Id, author.DisplayName, author.AvatarPath, post.Content, post.PhotoPath, post.CreatedAtUtc, 0, false);
    }

    public async Task<PagedResultDto<PostSummaryDto>> GetFeedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var clampedPage = page < 1 ? 1 : page;
        var clampedPageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        // Posts de quem bloqueei (ou que me bloqueou) somem do feed — mesma consistência
        // já aplicada na busca de jogadores e na lista de conversas.
        var blockedOrBlockingIds = await _blockRepository.GetBlockedOrBlockingProfileIdsAsync(profile.Id, cancellationToken);

        var (items, totalCount) = await _feedRepository.GetFeedAsync(blockedOrBlockingIds, profile.Id, clampedPage, clampedPageSize, cancellationToken);

        return new PagedResultDto<PostSummaryDto>(items, totalCount, clampedPage, clampedPageSize);
    }

    public async Task DeletePostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);
        var post = await _feedRepository.GetPostByIdAsync(postId, cancellationToken);

        // "Não existe" cobre tanto post inexistente quanto post de outra pessoa — não
        // confirma pra quem pergunta se um post alheio existe (mesmo padrão já usado em
        // conversas/perfis).
        if (post is null || post.AuthorProfileId != profile.Id)
        {
            throw new NotFoundException(nameof(Post), postId);
        }

        await _feedRepository.DeletePostAsync(post, cancellationToken);
    }

    public async Task<LikeToggleResultDto> ToggleLikeAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var post = await _feedRepository.GetPostByIdAsync(postId, cancellationToken)
            ?? throw new NotFoundException(nameof(Post), postId);

        var existingLike = await _feedRepository.FindLikeAsync(post.Id, profile.Id, cancellationToken);

        if (existingLike is null)
        {
            await _feedRepository.AddLikeAsync(PostLike.Create(post.Id, profile.Id), cancellationToken);
        }
        else
        {
            await _feedRepository.RemoveLikeAsync(existingLike, cancellationToken);
        }

        var likeCount = await _feedRepository.GetLikeCountAsync(post.Id, cancellationToken);
        return new LikeToggleResultDto(existingLike is null, likeCount);
    }

    private async Task<PlayerProfile> GetProfileOrThrowAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId);
    }
}

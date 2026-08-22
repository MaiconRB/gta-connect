using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Feed;
using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class FeedRepository : IFeedRepository
{
    private readonly AppDbContext _dbContext;

    public FeedRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddPostAsync(Post post, CancellationToken cancellationToken = default)
    {
        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Posts.SingleOrDefaultAsync(p => p.Id == postId, cancellationToken);
    }

    public async Task DeletePostAsync(Post post, CancellationToken cancellationToken = default)
    {
        _dbContext.Posts.Remove(post);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<PostSummaryDto> Items, int TotalCount)> GetFeedAsync(
        IReadOnlyCollection<Guid> excludedProfileIds,
        IReadOnlyCollection<Guid>? onlyProfileIds,
        Guid viewerProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from p in _dbContext.Posts
            where !excludedProfileIds.Contains(p.AuthorProfileId)
            where onlyProfileIds == null || onlyProfileIds.Contains(p.AuthorProfileId)
            join author in _dbContext.PlayerProfiles on p.AuthorProfileId equals author.Id
            select new { Post = p, Author = author };

        var totalCount = await query.CountAsync(cancellationToken);

        // Correlated subqueries (contagem de curtidas, "curti isso") só rodam pra página
        // atual — o Select de projeção vem DEPOIS do Skip/Take de propósito.
        var items = await query
            .OrderByDescending(x => x.Post.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PostSummaryDto(
                x.Post.Id,
                x.Post.AuthorProfileId,
                x.Author.DisplayName,
                x.Author.AvatarPath,
                x.Post.Content,
                x.Post.PhotoPath,
                x.Post.CreatedAtUtc,
                _dbContext.PostLikes.Count(l => l.PostId == x.Post.Id),
                _dbContext.PostLikes.Any(l => l.PostId == x.Post.Id && l.ProfileId == viewerProfileId)))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<PostSummaryDto>> GetByAuthorAsync(Guid authorProfileId, int limit, CancellationToken cancellationToken = default)
    {
        var query =
            from p in _dbContext.Posts
            where p.AuthorProfileId == authorProfileId
            join author in _dbContext.PlayerProfiles on p.AuthorProfileId equals author.Id
            orderby p.CreatedAtUtc descending
            select new PostSummaryDto(
                p.Id,
                p.AuthorProfileId,
                author.DisplayName,
                author.AvatarPath,
                p.Content,
                p.PhotoPath,
                p.CreatedAtUtc,
                _dbContext.PostLikes.Count(l => l.PostId == p.Id),
                false);

        return await query.Take(limit).ToListAsync(cancellationToken);
    }

    public Task<PostLike?> FindLikeAsync(Guid postId, Guid profileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PostLikes.SingleOrDefaultAsync(l => l.PostId == postId && l.ProfileId == profileId, cancellationToken);
    }

    public async Task AddLikeAsync(PostLike like, CancellationToken cancellationToken = default)
    {
        _dbContext.PostLikes.Add(like);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveLikeAsync(PostLike like, CancellationToken cancellationToken = default)
    {
        _dbContext.PostLikes.Remove(like);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> GetLikeCountAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PostLikes.CountAsync(l => l.PostId == postId, cancellationToken);
    }
}

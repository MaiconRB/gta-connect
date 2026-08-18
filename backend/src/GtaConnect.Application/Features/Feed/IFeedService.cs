using GtaConnect.Application.Common;

namespace GtaConnect.Application.Features.Feed;

public interface IFeedService
{
    Task<PostSummaryDto> CreatePostAsync(
        Guid userId,
        string? content,
        Stream? photoContent,
        string? photoFileName,
        long photoContentLength,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<PostSummaryDto>> GetFeedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Só o autor pode apagar — "não existe" cobre tanto post inexistente quanto post alheio.</summary>
    Task DeletePostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);

    /// <summary>Idempotente na prática: curte se ainda não curtiu, descurte se já tinha curtido.</summary>
    Task<LikeToggleResultDto> ToggleLikeAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
}

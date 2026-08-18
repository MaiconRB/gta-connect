using GtaConnect.Application.Features.Feed;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

/// <summary>
/// Cobre Post e PostLike juntos, de propósito — mesmo padrão do IChatRepository
/// (Conversation+Message): são o mesmo agregado conceitual, e a listagem já projeta os dois juntos.
/// </summary>
public interface IFeedRepository
{
    Task AddPostAsync(Post post, CancellationToken cancellationToken = default);

    Task<Post?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default);

    Task DeletePostAsync(Post post, CancellationToken cancellationToken = default);

    /// <summary>Feed paginado, mais recente primeiro. Nenhum post de excludedProfileIds aparece. LikedByMe é calculado em relação a viewerProfileId.</summary>
    Task<(IReadOnlyList<PostSummaryDto> Items, int TotalCount)> GetFeedAsync(
        IReadOnlyCollection<Guid> excludedProfileIds,
        Guid viewerProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PostLike?> FindLikeAsync(Guid postId, Guid profileId, CancellationToken cancellationToken = default);

    Task AddLikeAsync(PostLike like, CancellationToken cancellationToken = default);

    Task RemoveLikeAsync(PostLike like, CancellationToken cancellationToken = default);

    Task<int> GetLikeCountAsync(Guid postId, CancellationToken cancellationToken = default);
}

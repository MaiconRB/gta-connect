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

    /// <summary>
    /// Feed paginado, mais recente primeiro. Nenhum post de excludedProfileIds aparece.
    /// Se onlyProfileIds não for null, só posts desses autores aparecem (feed filtrado por
    /// conexões) — null significa "todos" (feed público global, comportamento padrão).
    /// LikedByMe é calculado em relação a viewerProfileId.
    /// </summary>
    Task<(IReadOnlyList<PostSummaryDto> Items, int TotalCount)> GetFeedAsync(
        IReadOnlyCollection<Guid> excludedProfileIds,
        IReadOnlyCollection<Guid>? onlyProfileIds,
        Guid viewerProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Posts mais recentes de um autor — usado pelo painel de moderação, sem "curtido por mim" (sempre false, sem viewer real).</summary>
    Task<IReadOnlyList<PostSummaryDto>> GetByAuthorAsync(Guid authorProfileId, int limit, CancellationToken cancellationToken = default);

    Task<PostLike?> FindLikeAsync(Guid postId, Guid profileId, CancellationToken cancellationToken = default);

    Task AddLikeAsync(PostLike like, CancellationToken cancellationToken = default);

    Task RemoveLikeAsync(PostLike like, CancellationToken cancellationToken = default);

    Task<int> GetLikeCountAsync(Guid postId, CancellationToken cancellationToken = default);
}

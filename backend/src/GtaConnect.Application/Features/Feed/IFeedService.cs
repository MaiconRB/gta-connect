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

    /// <summary>onlyConnections=true restringe o feed a autores com quem tenho uma Connection aceita (reaproveita Reputation, sem sistema de "seguir" novo).</summary>
    Task<PagedResultDto<PostSummaryDto>> GetFeedAsync(Guid userId, int page, int pageSize, bool onlyConnections, CancellationToken cancellationToken = default);

    /// <summary>Só o autor pode apagar — "não existe" cobre tanto post inexistente quanto post alheio.</summary>
    Task DeletePostAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);

    /// <summary>Apaga sem checar autoria — só pro painel de moderação, protegido por role na Api.</summary>
    Task DeletePostAsModeratorAsync(Guid postId, CancellationToken cancellationToken = default);

    /// <summary>Idempotente na prática: curte se ainda não curtiu, descurte se já tinha curtido.</summary>
    Task<LikeToggleResultDto> ToggleLikeAsync(Guid userId, Guid postId, CancellationToken cancellationToken = default);
}

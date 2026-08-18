using GtaConnect.Domain.Enums;

namespace GtaConnect.Application.Features.Moderation;

public interface IModerationService
{
    /// <summary>Idempotente — bloquear quem já está bloqueado não lança erro.</summary>
    Task BlockAsync(Guid userId, Guid targetProfileId, CancellationToken cancellationToken = default);

    /// <summary>Idempotente — desbloquear quem não está bloqueado não lança erro.</summary>
    Task UnblockAsync(Guid userId, Guid targetProfileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BlockedProfileSummaryDto>> GetBlockedProfilesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task ReportAsync(Guid userId, Guid targetProfileId, ReportReason reason, string? details, CancellationToken cancellationToken = default);
}

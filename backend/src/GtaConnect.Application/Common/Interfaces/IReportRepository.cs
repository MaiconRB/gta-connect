using GtaConnect.Application.Features.ModerationReview;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);

    Task<Report?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Report report, CancellationToken cancellationToken = default);

    /// <summary>Perfis com pelo menos uma denúncia, agrupados, com contagem de pendentes/total e status de banimento — uma query só.</summary>
    Task<IReadOnlyList<ReportedProfileSummaryDto>> GetGroupedByReportedProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>Todas as denúncias contra um perfil, mais recente primeiro, já com o nome de quem denunciou resolvido.</summary>
    Task<IReadOnlyList<ReportDetailDto>> GetByReportedProfileAsync(Guid reportedProfileId, CancellationToken cancellationToken = default);
}

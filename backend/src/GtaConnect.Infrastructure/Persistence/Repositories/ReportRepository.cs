using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.ModerationReview;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _dbContext;

    public ReportRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Report?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Reports.SingleOrDefaultAsync(r => r.Id == reportId, cancellationToken);
    }

    public async Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
    {
        // "report" já vem rastreado (mesmo DbContext/escopo) — nada de .Update() aqui,
        // mesmo motivo já documentado em ChatRepository/ConnectionRepository.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReportedProfileSummaryDto>> GetGroupedByReportedProfileAsync(CancellationToken cancellationToken = default)
    {
        var grouped =
            from r in _dbContext.Reports
            group r by r.ReportedProfileId into g
            select new
            {
                ReportedProfileId = g.Key,
                PendingCount = g.Count(x => x.Status == ReportStatus.Pending),
                TotalCount = g.Count(),
            };

        // Users vem do IdentityDbContext (AspNetUsers) — cruzado só pra saber se a conta
        // está banida (LockoutEnd), sem precisar de N chamadas a IIdentityService.
        var query =
            from g in grouped
            join p in _dbContext.PlayerProfiles on g.ReportedProfileId equals p.Id
            join u in _dbContext.Users on p.ApplicationUserId equals u.Id
            orderby g.PendingCount descending, g.TotalCount descending
            select new ReportedProfileSummaryDto(
                p.Id,
                p.DisplayName,
                p.AvatarPath,
                g.PendingCount,
                g.TotalCount,
                u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReportDetailDto>> GetByReportedProfileAsync(Guid reportedProfileId, CancellationToken cancellationToken = default)
    {
        var query =
            from r in _dbContext.Reports
            where r.ReportedProfileId == reportedProfileId
            join reporter in _dbContext.PlayerProfiles on r.ReporterProfileId equals reporter.Id
            orderby r.CreatedAtUtc descending
            select new ReportDetailDto(r.Id, reporter.DisplayName, r.Reason, r.Details, r.Status, r.CreatedAtUtc, r.ReviewedAtUtc);

        return await query.ToListAsync(cancellationToken);
    }
}

using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Domain.Entities;

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
}

using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

// Só escrita, de propósito — não há tela de revisão/consumidor de denúncias ainda.
public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
}

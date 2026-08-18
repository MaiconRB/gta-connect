using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.Reputation;
using GtaConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class ConnectionRepository : IConnectionRepository
{
    private readonly AppDbContext _dbContext;

    public ConnectionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Um Declined antigo não bloqueia um pedido novo (ver ConnectionService), então pode
    // existir mais de uma Connection histórica entre o mesmo par — pega sempre a mais
    // recente (SingleOrDefaultAsync lançaria exceção nesse caso).
    public Task<Connection?> FindBetweenAsync(Guid profileAId, Guid profileBId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Connections
            .Where(c => (c.RequesterProfileId == profileAId && c.AddresseeProfileId == profileBId)
                || (c.RequesterProfileId == profileBId && c.AddresseeProfileId == profileAId))
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Connection?> GetByIdAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Connections.SingleOrDefaultAsync(c => c.Id == connectionId, cancellationToken);
    }

    public async Task AddAsync(Connection connection, CancellationToken cancellationToken = default)
    {
        _dbContext.Connections.Add(connection);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Connection connection, CancellationToken cancellationToken = default)
    {
        // "connection" já vem rastreado (veio de GetByIdAsync, mesmo DbContext/escopo) —
        // o change tracker do EF já detecta sozinho que só Status/RespondedAtUtc mudaram.
        // Nada de .Update() aqui (mesmo motivo já documentado em ChatRepository).
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConnectionSummaryDto>> GetConnectionsForProfileAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var query =
            from c in _dbContext.Connections
            where c.RequesterProfileId == profileId || c.AddresseeProfileId == profileId
            let otherProfileId = c.RequesterProfileId == profileId ? c.AddresseeProfileId : c.RequesterProfileId
            let isRequester = c.RequesterProfileId == profileId
            join other in _dbContext.PlayerProfiles on otherProfileId equals other.Id
            orderby c.CreatedAtUtc descending
            select new ConnectionSummaryDto(c.Id, other.Id, other.DisplayName, other.AvatarPath, c.Status, isRequester, c.CreatedAtUtc);

        return await query.ToListAsync(cancellationToken);
    }
}

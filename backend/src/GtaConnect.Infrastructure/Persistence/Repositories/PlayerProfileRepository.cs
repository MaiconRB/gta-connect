using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Features.PlayerSearch;
using GtaConnect.Domain.Entities;
using GtaConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GtaConnect.Infrastructure.Persistence.Repositories;

public class PlayerProfileRepository : IPlayerProfileRepository
{
    private readonly AppDbContext _dbContext;

    public PlayerProfileRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        _dbContext.PlayerProfiles.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<PlayerProfile?> GetByUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PlayerProfiles
            .SingleOrDefaultAsync(p => p.ApplicationUserId == applicationUserId, cancellationToken);
    }

    public Task<PlayerProfile?> GetByIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PlayerProfiles
            .SingleOrDefaultAsync(p => p.Id == profileId, cancellationToken);
    }

    public async Task UpdateAsync(PlayerProfile profile, CancellationToken cancellationToken = default)
    {
        _dbContext.PlayerProfiles.Update(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<PlayerProfile> Items, int TotalCount)> SearchAsync(PlayerSearchFilterDto filter, Guid excludeProfileId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PlayerProfiles.AsQueryable();

        // Exclusão do próprio usuário logado acontece aqui, ANTES de Count/Skip/Take —
        // filtrar isso depois em memória quebraria a contagem total e a paginação.
        query = query.Where(p => p.Id != excludeProfileId);

        if (filter.Platform is not null)
        {
            query = query.Where(p => p.Platform == filter.Platform);
        }

        if (filter.Region is not null)
        {
            query = query.Where(p => p.Region == filter.Region);
        }

        // Bitwise "&" direto na expressão LINQ — traduz pro operador nativo do SQL Server.
        // NUNCA usar Enum.HasFlag aqui: não é traduzido pelo EF Core, cai em avaliação
        // client-side (traz a tabela inteira pra memória antes de filtrar).
        if (filter.PlaystyleTags != PlaystyleTag.None)
        {
            query = query.Where(p => (p.PlaystyleTags & filter.PlaystyleTags) != 0);
        }

        if (filter.AvailabilityTags != AvailabilityTag.None)
        {
            query = query.Where(p => (p.AvailabilityTags & filter.AvailabilityTags) != 0);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

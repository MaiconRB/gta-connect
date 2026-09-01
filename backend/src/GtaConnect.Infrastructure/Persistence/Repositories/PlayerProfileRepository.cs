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

    public async Task<(IReadOnlyList<PlayerProfile> Items, int TotalCount)> SearchAsync(
        PlayerSearchFilterDto filter,
        IReadOnlyCollection<Guid> excludedProfileIds,
        PlaystyleTag myPlaystyleTags,
        AvailabilityTag myAvailabilityTags,
        Region? myRegion,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PlayerProfiles.AsQueryable();

        // Exclusão do próprio usuário logado + bloqueados acontece aqui, ANTES de
        // Count/Skip/Take — filtrar isso depois em memória quebraria a contagem total e a paginação.
        query = query.Where(p => !excludedProfileIds.Contains(p.Id));

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

        // Score de compatibilidade — ordena a busca em vez do antigo OrderByDescending(CreatedAtUtc).
        // Tudo escrito pra rodar NO BANCO (nada de .AsEnumerable()/client-eval no meio):
        //
        //  - Confiabilidade (peso 40): média de CompletedSession recebidas, via subquery
        //    correlacionada em Ratings. Perfil sem avaliação nenhuma usa 0.6 como neutro —
        //    não afunda quem é novo, mas também não empata artificialmente com quem já provou
        //    ser confiável.
        //  - Sobreposição de PlaystyleTags (peso 3 por tag em comum) e AvailabilityTags (peso 3):
        //    contagem de bits em comum via CASE explícito por bit — SQL Server não tem BIT_COUNT
        //    traduzível pelo EF Core, então cada tag vira um CASE WHEN separado (ver PROJETO.md,
        //    é um dos motivos pra migrar pra Postgres no futuro).
        //  - Mesma região (peso 10).
        //
        // IsOnline fica DE FORA do score: presença vive só em memória (InMemoryPresenceTracker),
        // não no banco — não dá pra ordenar por ela sem trazer tudo pra client-eval. Continua
        // como selo visual no card, não como critério de ranking.
        var items = await query
            .OrderByDescending(p =>
                (40.0 * (_dbContext.Ratings
                    .Where(r => r.RatedProfileId == p.Id)
                    .Select(r => (double?)(r.CompletedSession ? 1.0 : 0.0))
                    .Average() ?? 0.6))
                + (3 * (
                    ((p.PlaystyleTags & myPlaystyleTags & PlaystyleTag.MundoAbertoCalmo) != 0 ? 1 : 0) +
                    ((p.PlaystyleTags & myPlaystyleTags & PlaystyleTag.GrindDeHeist) != 0 ? 1 : 0) +
                    ((p.PlaystyleTags & myPlaystyleTags & PlaystyleTag.Corrida) != 0 ? 1 : 0) +
                    ((p.PlaystyleTags & myPlaystyleTags & PlaystyleTag.RolePlay) != 0 ? 1 : 0) +
                    ((p.PlaystyleTags & myPlaystyleTags & PlaystyleTag.FreemodeSocial) != 0 ? 1 : 0) +
                    ((p.PlaystyleTags & myPlaystyleTags & PlaystyleTag.CampanhaHistoria) != 0 ? 1 : 0) +
                    ((p.PlaystyleTags & myPlaystyleTags & PlaystyleTag.PvpCompetitivo) != 0 ? 1 : 0) +
                    ((p.PlaystyleTags & myPlaystyleTags & PlaystyleTag.NegociosEconomia) != 0 ? 1 : 0)))
                + (3 * (
                    ((p.AvailabilityTags & myAvailabilityTags & AvailabilityTag.Manha) != 0 ? 1 : 0) +
                    ((p.AvailabilityTags & myAvailabilityTags & AvailabilityTag.Tarde) != 0 ? 1 : 0) +
                    ((p.AvailabilityTags & myAvailabilityTags & AvailabilityTag.Noite) != 0 ? 1 : 0) +
                    ((p.AvailabilityTags & myAvailabilityTags & AvailabilityTag.Madrugada) != 0 ? 1 : 0) +
                    ((p.AvailabilityTags & myAvailabilityTags & AvailabilityTag.FimDeSemana) != 0 ? 1 : 0)))
                + (myRegion != null && p.Region == myRegion ? 10 : 0))
            .ThenByDescending(p => p.CreatedAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

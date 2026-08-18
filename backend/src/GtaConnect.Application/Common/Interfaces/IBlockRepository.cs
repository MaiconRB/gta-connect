using GtaConnect.Application.Features.Moderation;
using GtaConnect.Domain.Entities;

namespace GtaConnect.Application.Common.Interfaces;

public interface IBlockRepository
{
    /// <summary>Existe bloqueio entre os dois perfis, em qualquer direção — usado pra checagens de efeito (busca, chat).</summary>
    Task<bool> ExistsEitherDirectionAsync(Guid profileAId, Guid profileBId, CancellationToken cancellationToken = default);

    /// <summary>Direção exata — só o bloqueio que o próprio blocker criou pode ser desfeito por ele.</summary>
    Task<Block?> FindAsync(Guid blockerProfileId, Guid blockedProfileId, CancellationToken cancellationToken = default);

    Task AddAsync(Block block, CancellationToken cancellationToken = default);

    Task RemoveAsync(Block block, CancellationToken cancellationToken = default);

    /// <summary>Todo mundo que o perfil bloqueou OU que bloqueou o perfil — usado pra excluir da busca/lista de conversas.</summary>
    Task<IReadOnlyList<Guid>> GetBlockedOrBlockingProfileIdsAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>Só quem o próprio perfil bloqueou (não quem o bloqueou) — pra tela de gerenciamento.</summary>
    Task<IReadOnlyList<BlockedProfileSummaryDto>> GetBlockedProfilesAsync(Guid blockerProfileId, CancellationToken cancellationToken = default);
}

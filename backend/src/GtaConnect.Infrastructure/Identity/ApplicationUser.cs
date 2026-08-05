using Microsoft.AspNetCore.Identity;

namespace GtaConnect.Infrastructure.Identity;

/// <summary>
/// Usuário de autenticação (Identity). Fica na Infrastructure de propósito: autenticação
/// é uma preocupação de infraestrutura, não faz parte do Domain. O "quem o jogador é
/// dentro do produto" mora em GtaConnect.Domain.Entities.PlayerProfile, ligado a este
/// usuário pelo Id.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}

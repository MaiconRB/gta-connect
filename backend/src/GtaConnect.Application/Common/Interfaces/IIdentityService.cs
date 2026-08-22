namespace GtaConnect.Application.Common.Interfaces;

/// <summary>
/// Abstrai o ASP.NET Core Identity (que vive na Infrastructure) para a Application.
/// A Application nao conhece "ApplicationUser", "UserManager" nem nada do Identity —
/// so sabe que existe alguem que sabe criar usuarios e validar credenciais.
/// </summary>
public interface IIdentityService
{
    Task<CreateUserResult> CreateUserAsync(string email, string password);

    /// <summary>Retorna o Id do usuario se as credenciais forem validas, ou null caso contrario.</summary>
    Task<Guid?> ValidateCredentialsAsync(string email, string password);

    /// <summary>Retorna o e-mail do usuario pelo Id, ou null se nao encontrado.</summary>
    Task<string?> GetUserEmailAsync(Guid userId);

    /// <summary>Retorna true se o e-mail do usuario ja foi confirmado.</summary>
    Task<bool> IsEmailConfirmedAsync(Guid userId);

    /// <summary>Gera um token de confirmacao de e-mail para o usuario codificado em Base64Url (seguro para URL).</summary>
    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);

    /// <summary>Confirma o e-mail do usuario com o token Base64Url fornecido. Retorna true em caso de sucesso.</summary>
    Task<bool> ConfirmEmailAsync(Guid userId, string token);

    /// <summary>Concede ou revoga a role "Moderator" pra deixar em sincronia com shouldBeModerator. Retorna o status final.</summary>
    Task<bool> SyncModeratorRoleAsync(Guid userId, bool shouldBeModerator);

    /// <summary>Bane o usuario permanentemente (lockout nativo do Identity).</summary>
    Task BanUserAsync(Guid userId);

    Task UnbanUserAsync(Guid userId);

    Task<bool> IsUserBannedAsync(Guid userId);
}

public record CreateUserResult(bool Succeeded, Guid? UserId, IReadOnlyList<string> Errors);

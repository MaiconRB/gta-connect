namespace GtaConnect.Application.Common.Interfaces;

/// <summary>
/// Abstrai o ASP.NET Core Identity (que vive na Infrastructure) para a Application.
/// A Application não conhece "ApplicationUser", "UserManager" nem nada do Identity —
/// só sabe que existe alguém que sabe criar usuários e validar credenciais.
/// </summary>
public interface IIdentityService
{
    Task<CreateUserResult> CreateUserAsync(string email, string password);

    /// <summary>Retorna o Id do usuário se as credenciais forem válidas, ou null caso contrário.</summary>
    Task<Guid?> ValidateCredentialsAsync(string email, string password);
}

public record CreateUserResult(bool Succeeded, Guid? UserId, IReadOnlyList<string> Errors);

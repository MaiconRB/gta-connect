using GtaConnect.Application.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Infrastructure.Identity;

/// <summary>
/// O ASP.NET Core Identity usa este ponto de extensão (Strategy/Template Method) para
/// customizar as mensagens de erro que normalmente vêm fixas em inglês. Só sobrescreve os
/// métodos que a configuração atual do Identity (ver DependencyInjection.cs, RequireNonAlphanumeric
/// = false) realmente pode disparar — sobrescrever o resto seria trabalho morto.
/// </summary>
public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LocalizedIdentityErrorDescriber(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = _localizer["Password_RequiresUppercase"],
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = _localizer["Password_RequiresLowercase"],
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = _localizer["Password_RequiresDigit"],
    };

    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = _localizer["Identity_PasswordTooShort", length],
    };

    public override IdentityError DuplicateEmail(string email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = _localizer["Identity_DuplicateEmail"],
    };

    // O app usa o email como UserName (ver IdentityService.CreateUserAsync) — não existe
    // conceito de "username" separado na UI. O Identity valida username e email como
    // unicidades distintas, então os dois erros disparam juntos num email duplicado;
    // sem isto, "DuplicateUserName" vazaria sempre em inglês, fixo, independente da cultura.
    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = _localizer["Identity_DuplicateEmail"],
    };

    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = _localizer["Identity_InvalidEmail"],
    };
}

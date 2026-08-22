using GtaConnect.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace GtaConnect.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private const string ModeratorRoleName = "Moderator";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<CreateUserResult> CreateUserAsync(string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return new CreateUserResult(false, null, errors);
        }

        return new CreateUserResult(true, user.Id, []);
    }

    public async Task<Guid?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        return isPasswordValid ? user.Id : null;
    }

    public async Task<string?> GetUserEmailAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.Email;
    }

    public async Task<bool> IsEmailConfirmedAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && user.EmailConfirmed;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"Usuario {userId} nao encontrado.");

        var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Tokens do Identity contem caracteres especiais (+, /, =) que quebram URLs.
        // WebEncoders.Base64UrlEncode produz um token seguro para query string.
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string encodedToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        // Reverter a codificacao Base64Url feita em GenerateEmailConfirmationTokenAsync.
        string rawToken;
        try
        {
            rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
        }
        catch
        {
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, rawToken);
        return result.Succeeded;
    }

    public async Task<bool> SyncModeratorRoleAsync(Guid userId, bool shouldBeModerator)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        var isCurrentlyModerator = await _userManager.IsInRoleAsync(user, ModeratorRoleName);

        if (shouldBeModerator && !isCurrentlyModerator)
        {
            // Garante que a role existe antes de atribuir — RoleManager não cria sozinho.
            if (!await _roleManager.RoleExistsAsync(ModeratorRoleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(ModeratorRoleName));
            }

            await _userManager.AddToRoleAsync(user, ModeratorRoleName);
        }
        else if (!shouldBeModerator && isCurrentlyModerator)
        {
            await _userManager.RemoveFromRoleAsync(user, ModeratorRoleName);
        }

        return shouldBeModerator;
    }

    public async Task BanUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return;
        }

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
    }

    public async Task UnbanUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return;
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
    }

    public async Task<bool> IsUserBannedAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && await _userManager.IsLockedOutAsync(user);
    }
}

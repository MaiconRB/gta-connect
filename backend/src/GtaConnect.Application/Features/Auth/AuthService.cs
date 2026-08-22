using FluentValidation;
using GtaConnect.Application.Common;
using GtaConnect.Application.Common.Extensions;
using GtaConnect.Application.Common.Interfaces;
using GtaConnect.Application.Resources;
using GtaConnect.Domain.Common.Exceptions;
using GtaConnect.Domain.Entities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace GtaConnect.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly string _frontendUrl;
    private readonly string[] _moderatorEmails;

    public AuthService(
        IIdentityService identityService,
        IPlayerProfileRepository playerProfileRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IStringLocalizer<SharedResource> localizer,
        IOptions<AuthOptions> authOptions)
    {
        _identityService = identityService;
        _playerProfileRepository = playerProfileRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _localizer = localizer;
        _frontendUrl = authOptions.Value.FrontendUrl;
        _moderatorEmails = authOptions.Value.ModeratorEmails;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAppExceptionAsync(request, cancellationToken);

        var createResult = await _identityService.CreateUserAsync(request.Email, request.Password);
        if (!createResult.Succeeded)
        {
            throw new ValidationAppException("email", string.Join(" ", createResult.Errors.Distinct()));
        }

        var profile = PlayerProfile.Create(createResult.UserId!.Value, request.DisplayName, request.Platform, request.GameTitle);
        await _playerProfileRepository.AddAsync(profile, cancellationToken);

        // Envia o e-mail de confirmacao de forma fire-and-forget:
        // uma falha no envio nao impede o cadastro — o usuario pode pedir reenvio depois.
        _ = SendConfirmationEmailAsync(createResult.UserId.Value, request.Email, CancellationToken.None);

        var isModerator = await _identityService.SyncModeratorRoleAsync(createResult.UserId.Value, IsConfiguredModeratorEmail(request.Email));

        var token = _jwtTokenGenerator.GenerateToken(createResult.UserId.Value, request.Email, profile.DisplayName, RolesFor(isModerator));
        return new AuthResponseDto(token.Value, token.ExpiresAtUtc, request.Email, profile.DisplayName, EmailConfirmed: false, isModerator);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAppExceptionAsync(request, cancellationToken);

        var userId = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
        if (userId is null)
        {
            throw new ValidationAppException("credentials", _localizer["InvalidCredentials"]);
        }

        if (await _identityService.IsUserBannedAsync(userId.Value))
        {
            throw new ValidationAppException("credentials", _localizer["Account_Banned"]);
        }

        var profile = await _playerProfileRepository.GetByUserIdAsync(userId.Value, cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerProfile), userId.Value);

        var emailConfirmed = await _identityService.IsEmailConfirmedAsync(userId.Value);
        var isModerator = await _identityService.SyncModeratorRoleAsync(userId.Value, IsConfiguredModeratorEmail(request.Email));

        var token = _jwtTokenGenerator.GenerateToken(userId.Value, request.Email, profile.DisplayName, RolesFor(isModerator));
        return new AuthResponseDto(token.Value, token.ExpiresAtUtc, request.Email, profile.DisplayName, emailConfirmed, isModerator);
    }

    private bool IsConfiguredModeratorEmail(string email) =>
        _moderatorEmails.Contains(email, StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyList<string> RolesFor(bool isModerator) =>
        isModerator ? ["Moderator"] : [];

    public async Task ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default)
    {
        var confirmed = await _identityService.ConfirmEmailAsync(userId, token);
        if (!confirmed)
        {
            throw new ValidationAppException("token", _localizer["Email_InvalidConfirmationToken"]);
        }
    }

    public async Task ResendConfirmationEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var alreadyConfirmed = await _identityService.IsEmailConfirmedAsync(userId);
        if (alreadyConfirmed)
        {
            // Silenciosamente ignorar: nao expor detalhes de estado da conta por seguranca.
            return;
        }

        var email = await _identityService.GetUserEmailAsync(userId)
            ?? throw new NotFoundException("User", userId);

        await SendConfirmationEmailAsync(userId, email, cancellationToken);
    }

    private async Task SendConfirmationEmailAsync(Guid userId, string email, CancellationToken cancellationToken)
    {
        try
        {
            var encodedToken = await _identityService.GenerateEmailConfirmationTokenAsync(userId);
            var confirmUrl = $"{_frontendUrl}/confirmar-email?userId={userId}&token={Uri.EscapeDataString(encodedToken)}";

            var subject = "Confirme seu e-mail — GTA Connect";
            var html = BuildConfirmationEmailHtml(confirmUrl);

            await _emailService.SendEmailAsync(email, subject, html, cancellationToken);
        }
        catch
        {
            // Falha no envio nao deve derrubar o fluxo principal.
            // O usuario pode pedir reenvio depois pelo banner no app.
        }
    }

    private static string BuildConfirmationEmailHtml(string confirmUrl) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <body style="margin:0;padding:0;background:#0A0612;font-family:system-ui,sans-serif;color:#e5e5e5;">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr><td align="center" style="padding:40px 16px;">
              <table width="560" cellpadding="0" cellspacing="0" style="background:#13101f;border-radius:12px;border:1px solid #2a2040;">
                <tr><td style="padding:40px 40px 24px;">
                  <h1 style="margin:0 0 8px;font-size:24px;color:#FF3EC9;font-weight:700;">GTA Connect</h1>
                  <p style="margin:0 0 24px;color:#a0a0b0;font-size:14px;">Confirme seu e-mail para garantir acesso completo</p>
                  <p style="margin:0 0 32px;line-height:1.6;">
                    Clique no botao abaixo para confirmar seu e-mail e ativar sua conta.
                  </p>
                  <a href="{confirmUrl}"
                     style="display:inline-block;padding:14px 32px;background:#FF3EC9;color:#0A0612;font-weight:700;font-size:15px;border-radius:8px;text-decoration:none;">
                    Confirmar e-mail
                  </a>
                  <p style="margin:32px 0 0;font-size:12px;color:#6060a0;">
                    Se voce nao criou uma conta no GTA Connect, ignore este e-mail.
                    Este link expira em 24 horas.
                  </p>
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}

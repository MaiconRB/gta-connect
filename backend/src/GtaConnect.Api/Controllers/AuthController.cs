using System.Security.Claims;
using GtaConnect.Api.Common;
using GtaConnect.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> Me()
    {
        var userId = User.GetUserId();
        var email = User.FindFirstValue(ClaimTypes.Email);
        var displayName = User.FindFirstValue("display_name");

        return Ok(new { Id = userId, Email = email, DisplayName = displayName });
    }

    /// <summary>
    /// Confirma o e-mail do usuario a partir do link enviado por e-mail.
    /// Rota anonima: o usuario pode nao estar logado ao clicar no link.
    /// </summary>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(
        [FromQuery] Guid userId,
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        await _authService.ConfirmEmailAsync(userId, token, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Reenvia o e-mail de confirmacao para o usuario autenticado.
    /// Idempotente: se o e-mail ja foi confirmado, retorna 200 silenciosamente.
    /// </summary>
    [HttpPost("resend-confirmation")]
    [Authorize]
    public async Task<ActionResult> ResendConfirmation(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _authService.ResendConfirmationEmailAsync(userId, cancellationToken);
        return Ok();
    }
}

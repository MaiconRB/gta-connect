using System.Security.Claims;

namespace GtaConnect.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extrai o Id do usuário autenticado do token JWT. Só deve ser chamado em endpoints
    /// [Authorize] — nesse caso o claim sempre existe e é um Guid válido (é o próprio
    /// JwtTokenGenerator quem o emite), então uma falha aqui indica bug de configuração,
    /// não entrada de usuário — por isso InvalidOperationException, não um 4xx "esperado".
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new InvalidOperationException("Claim de identificação do usuário ausente ou inválida no token.");
        }

        return userId;
    }
}

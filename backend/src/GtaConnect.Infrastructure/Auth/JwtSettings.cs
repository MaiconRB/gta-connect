using System.ComponentModel.DataAnnotations;

namespace GtaConnect.Infrastructure.Auth;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    public required string Issuer { get; set; }

    [Required]
    public required string Audience { get; set; }

    /// <summary>Chave usada para assinar o token. Vem de user-secrets/variável de ambiente, nunca do appsettings.json.</summary>
    [Required, MinLength(32, ErrorMessage = "A chave JWT deve ter ao menos 32 caracteres.")]
    public required string SecretKey { get; set; }

    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; } = 60;
}

using System.ComponentModel.DataAnnotations;

namespace GtaConnect.Infrastructure.Email;

/// <summary>
/// Configuracoes de envio de e-mail. Em desenvolvimento aponta para o Mailpit local;
/// em producao, para qualquer servidor SMTP real (Resend, SendGrid, etc.).
/// Segue o mesmo padrao de EmailSettings/JwtSettings ja existente no projeto.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    [Required]
    public string SmtpHost { get; init; } = string.Empty;

    [Required]
    public int SmtpPort { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    [Required]
    public string FromEmail { get; init; } = string.Empty;

    [Required]
    public string FromName { get; init; } = string.Empty;

    public bool UseSsl { get; init; }
}

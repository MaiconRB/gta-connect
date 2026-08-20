namespace GtaConnect.Application.Common.Interfaces;

/// <summary>
/// Abstrai o envio de e-mails para a Application.
/// A Application nao conhece SMTP, MailKit nem qualquer provedor externo —
/// so sabe que existe algo que sabe enviar e-mails.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

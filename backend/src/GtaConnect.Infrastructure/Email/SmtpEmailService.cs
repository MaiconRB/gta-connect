using GtaConnect.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GtaConnect.Infrastructure.Email;

/// <summary>
/// Implementacao de IEmailService usando MailKit.
/// Em desenvolvimento, aponta para o Mailpit local (localhost:1025, sem autenticacao).
/// Em producao, basta ajustar EmailSettings nas variaveis de ambiente — o codigo nao muda.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public SmtpEmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();

        // SecureSocketOptions.Auto: negocia TLS automaticamente.
        // Mailpit (dev) nao usa TLS — o Auto detecta isso e conecta sem criptografia.
        // Servidores reais (producao) negociam TLS normalmente.
        var socketOptions = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, socketOptions, cancellationToken);

        // Mailpit nao exige autenticacao — pular se nao houver credenciais configuradas.
        if (!string.IsNullOrEmpty(_settings.Username))
        {
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}

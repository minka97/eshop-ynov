using Email.API.Configuration;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Email.API.Services;

/// <summary>
/// Service d'envoi d'emails via SMTP avec MailKit
/// Configuré pour Mailpit en développement
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> smtpSettings, ILogger<SmtpEmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📧 Préparation de l'envoi d'email à {To}", to);

            // Créer le message MIME
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            // Créer le body HTML
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Envoyer via SMTP
            using var client = new SmtpClient();
            
            // Connexion au serveur SMTP (Mailpit)
            await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.EnableSsl, cancellationToken);

            // Authentification si nécessaire (Mailpit n'en a pas besoin)
            if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, cancellationToken);
            }

            // Envoi
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("✅ Email envoyé avec succès à {To}", to);
            _logger.LogInformation("📬 Consultez Mailpit sur http://localhost:8025 pour voir l'email");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de l'envoi d'email à {To}", to);
            return false;
        }
    }
}

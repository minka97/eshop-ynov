using Microsoft.Extensions.Logging;
using Ordering.Application.Services;

namespace Ordering.Infrastructure.Services;

/// <summary>
/// Implémentation du service d'envoi d'emails
/// Pour le TP, on simule l'envoi (log dans la console)
/// En production, utiliser SMTP, SendGrid, ou autre service
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        // Pour le TP : Simulation de l'envoi (log)
        _logger.LogInformation("📧 EMAIL ENVOYÉ");
        _logger.LogInformation("=====================================");
        _logger.LogInformation("À : {To}", to);
        _logger.LogInformation("Sujet : {Subject}", subject);
        _logger.LogInformation("-------------------------------------");
        _logger.LogInformation("{Body}", body);
        _logger.LogInformation("=====================================");

        // Simule un délai asynchrone d'envoi
        await Task.Delay(100);

        // En production, utiliser un vrai service SMTP :
        /*
        using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential("email@example.com", "password"),
            EnableSsl = true
        };
        
        var mailMessage = new MailMessage
        {
            From = new MailAddress("noreply@eshop.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);
        
        await smtpClient.SendMailAsync(mailMessage);
        */

        return true; // Succès
    }
}

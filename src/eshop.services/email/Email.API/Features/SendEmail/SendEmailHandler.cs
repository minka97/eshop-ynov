using BuildingBlocks.CQRS;
using Email.API.Services;

namespace Email.API.Features.SendEmail;

/// <summary>
/// Handler pour la commande SendEmailCommand
/// Gère l'envoi d'email via le service IEmailService
/// </summary>
public class SendEmailHandler : ICommandHandler<SendEmailCommand, SendEmailResult>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailHandler> _logger;

    public SendEmailHandler(IEmailService emailService, ILogger<SendEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SendEmailResult> Handle(SendEmailCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📨 Tentative d'envoi d'email manuel à {To}", command.To);

        var success = await _emailService.SendEmailAsync(
            command.To,
            command.Subject,
            command.Body,
            cancellationToken
        );

        if (success)
        {
            _logger.LogInformation("✅ Email envoyé avec succès");
            return new SendEmailResult(true, "Email envoyé avec succès");
        }
        else
        {
            _logger.LogWarning("⚠️ Échec de l'envoi d'email");
            return new SendEmailResult(false, "Échec de l'envoi d'email");
        }
    }
}

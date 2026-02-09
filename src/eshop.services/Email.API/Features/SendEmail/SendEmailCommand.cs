using BuildingBlocks.CQRS;

namespace Email.API.Features.SendEmail;

/// <summary>
/// Commande pour envoyer un email manuellement via l'API REST
/// </summary>
public record SendEmailCommand(
    string To,
    string Subject,
    string Body
) : ICommand<SendEmailResult>;

/// <summary>
/// Résultat de l'envoi d'email
/// </summary>
public record SendEmailResult(bool Success, string Message);

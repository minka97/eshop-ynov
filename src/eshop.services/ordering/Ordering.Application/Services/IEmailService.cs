namespace Ordering.Application.Services;

/// <summary>
/// Interface pour le service d'envoi d'emails
/// Abstraction pour respecter Clean Architecture
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envoie un email
    /// </summary>
    /// <param name="to">Adresse email du destinataire</param>
    /// <param name="subject">Sujet de l'email</param>
    /// <param name="body">Contenu HTML de l'email</param>
    Task<bool> SendEmailAsync(string to, string subject, string body);
}

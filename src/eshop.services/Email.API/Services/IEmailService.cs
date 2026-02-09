namespace Email.API.Services;

/// <summary>
/// Interface pour le service d'envoi d'emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envoie un email de manière asynchrone
    /// </summary>
    /// <param name="to">Adresse email du destinataire</param>
    /// <param name="subject">Sujet de l'email</param>
    /// <param name="body">Contenu HTML de l'email</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>True si l'envoi a réussi, False sinon</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

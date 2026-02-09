namespace Email.API.Models;

/// <summary>
/// Représente un message email à envoyer
/// </summary>
public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? From { get; set; }
    public bool IsHtml { get; set; } = true;
}

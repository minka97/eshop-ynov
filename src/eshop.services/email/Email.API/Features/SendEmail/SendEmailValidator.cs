using FluentValidation;

namespace Email.API.Features.SendEmail;

/// <summary>
/// Validation pour la commande SendEmailCommand
/// Vérifie le format des emails et les champs requis
/// </summary>
public class SendEmailValidator : AbstractValidator<SendEmailCommand>
{
    public SendEmailValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("L'adresse email du destinataire est requise")
            .EmailAddress().WithMessage("Format d'email invalide");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Le sujet est requis")
            .MaximumLength(200).WithMessage("Le sujet ne peut pas dépasser 200 caractères");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Le corps de l'email est requis");
    }
}

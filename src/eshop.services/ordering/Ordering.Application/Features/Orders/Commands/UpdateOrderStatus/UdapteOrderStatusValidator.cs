using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Vérifie que les données sont correctes
/// </summary>
public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusValidator()
    {
        // Règle 1 : OrderId ne doit pas être vide
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("L'ID de la commande est obligatoire");

        // Règle 2 : NewStatus doit être défini (enum valide)
        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Le statut doit être valide (Pending, Processing, Completed, Cancelled)");
    }
}
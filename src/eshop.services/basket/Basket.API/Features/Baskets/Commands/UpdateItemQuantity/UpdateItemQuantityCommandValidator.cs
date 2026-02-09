using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.UpdateItemQuantity;

/// <summary>
/// Validator for the UpdateItemQuantityCommand to ensure all required fields are valid.
/// </summary>
public class UpdateItemQuantityCommandValidator : AbstractValidator<UpdateItemQuantityCommand>
{
    public UpdateItemQuantityCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("UserName is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required and must be a valid GUID.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}

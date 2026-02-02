using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Validator for the DeleteBasketItemCommand to ensure all required fields are valid.
/// </summary>
public class DeleteBasketItemCommandValidator : AbstractValidator<DeleteBasketItemCommand>
{
    public DeleteBasketItemCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("UserName is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required and must be a valid GUID.");
    }
}

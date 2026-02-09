using FluentValidation;

namespace Basket.API.Features.Baskets.Commands.AddItemInCart;

/// <summary>
/// Validator for the <see cref="AddItemInCartCommand"/>.
/// </summary>
/// <remarks>
/// Provides validation rules for the command to ensure required data is provided
/// and adheres to business constraints when adding an item to a shopping cart.
/// Validates the quantity of the item and the username associated with the cart.
/// </remarks>
public class AddItemInCartCommandValidator : AbstractValidator<AddItemInCartCommand>
{
    /// <summary>
    /// Validates the <see cref="AddItemInCartCommand"/> to ensure that the input data is valid.
    /// </summary>
    /// <remarks>
    /// This validator ensures the following rules are adhered to:
    /// - The quantity of the item in the shopping cart must not be zero.
    /// - The username associated with the cart must not be empty.
    /// </remarks>
    public AddItemInCartCommandValidator()
    {
        RuleFor(command => command.Item.Quantity).NotEqual(0).WithMessage("Product is required");
        RuleFor(command => command.Username).NotEmpty().WithMessage("Username is required");
    }
}
using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.AddItemInCart;

/// <summary>
/// Represents the result of adding an item to a shopping cart.
/// </summary>
/// <remarks>
/// This record encapsulates the updated state of the user's shopping cart
/// after a successful execution of the AddItemInCartCommand.
/// </remarks>
public record AddItemInCartCommandResult(ShoppingCart ShoppingCart); 
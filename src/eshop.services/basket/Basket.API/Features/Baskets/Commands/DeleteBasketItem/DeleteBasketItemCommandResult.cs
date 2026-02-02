using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Represents the result of the DeleteBasketItem command operation.
/// </summary>
/// <remarks>
/// This record encapsulates the outcome of removing a specific item from a user's basket,
/// including the success status, the username, and the updated shopping cart.
/// </remarks>
/// <param name="IsSuccess">
/// Indicates whether the delete operation was successful.
/// </param>
/// <param name="UserName">
/// The username of the user whose basket was updated.
/// </param>
/// <param name="UpdatedCart">
/// The shopping cart after the item has been removed.
/// </param>
public record DeleteBasketItemCommandResult(
    bool IsSuccess,
    string UserName,
    ShoppingCart UpdatedCart
);

using Basket.API.Models;

namespace Basket.API.Features.Baskets.Commands.UpdateItemQuantity;

/// <summary>
/// Represents the result of the UpdateItemQuantity command operation.
/// </summary>
/// <remarks>
/// This record encapsulates the outcome of updating a product's quantity in a user's basket,
/// including the success status, the username, and the updated shopping cart.
/// </remarks>
/// <param name="IsSuccess">
/// Indicates whether the update operation was successful.
/// </param>
/// <param name="UserName">
/// The username of the user whose basket was updated.
/// </param>
/// <param name="UpdatedCart">
/// The shopping cart with the updated item quantity.
/// </param>
public record UpdateItemQuantityCommandResult(
    bool IsSuccess,
    string UserName,
    ShoppingCart UpdatedCart
);

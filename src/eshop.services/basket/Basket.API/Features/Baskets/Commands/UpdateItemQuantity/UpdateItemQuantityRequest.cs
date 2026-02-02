namespace Basket.API.Features.Baskets.Commands.UpdateItemQuantity;

/// <summary>
/// Request model for updating the quantity of a product in the basket.
/// </summary>
/// <param name="Quantity">
/// The new quantity for the product. Must be greater than 0.
/// </param>
public record UpdateItemQuantityRequest(int Quantity);

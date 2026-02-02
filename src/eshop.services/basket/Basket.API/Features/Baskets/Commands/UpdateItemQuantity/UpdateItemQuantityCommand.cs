using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.UpdateItemQuantity;

/// <summary>
/// A command to update the quantity of a specific item in a user's shopping basket.
/// </summary>
/// <remarks>
/// This command follows the CQRS pattern and is used to modify only the quantity of a specific product
/// without replacing the entire basket. It implements the <see cref="ICommand{TResponse}"/> interface,
/// where the response type is <see cref="UpdateItemQuantityCommandResult"/>.
/// </remarks>
/// <param name="UserName">
/// The username identifying the basket to be updated.
/// </param>
/// <param name="ProductId">
/// The unique identifier of the product whose quantity should be updated.
/// </param>
/// <param name="Quantity">
/// The new quantity for the specified product. Must be greater than 0.
/// </param>
public record UpdateItemQuantityCommand(
    string UserName,
    Guid ProductId,
    int Quantity
) : ICommand<UpdateItemQuantityCommandResult>;

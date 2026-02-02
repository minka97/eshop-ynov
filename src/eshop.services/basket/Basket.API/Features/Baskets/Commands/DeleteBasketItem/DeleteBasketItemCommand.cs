using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// A command to delete a specific item from a user's shopping basket.
/// </summary>
/// <remarks>
/// This command follows the CQRS pattern and allows removal of a single product from the basket
/// without deleting the entire basket. It implements the <see cref="ICommand{TResponse}"/> interface,
/// where the response type is <see cref="DeleteBasketItemCommandResult"/>.
/// </remarks>
/// <param name="UserName">
/// The username identifying the basket from which the item should be removed.
/// </param>
/// <param name="ProductId">
/// The unique identifier of the product to be removed from the basket.
/// </param>
public record DeleteBasketItemCommand(
    string UserName,
    Guid ProductId
) : ICommand<DeleteBasketItemCommandResult>;

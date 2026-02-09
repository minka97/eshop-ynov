using Basket.API.Data.Repositories;
using Basket.API.Exceptions;
using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.DeleteBasketItem;

/// <summary>
/// Handles the DeleteBasketItemCommand by removing a specific item from the user's basket.
/// </summary>
public class DeleteBasketItemCommandHandler(IBasketRepository repository) 
    : ICommandHandler<DeleteBasketItemCommand, DeleteBasketItemCommandResult>
{
    /// <summary>
    /// Handles the request to delete a specific item from the shopping basket.
    /// </summary>
    /// <param name="request">The DeleteBasketItemCommand containing the user and product information.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation, returning a DeleteBasketItemCommandResult.</returns>
    /// <exception cref="BasketNotFoundException">Thrown when no basket is found for the specified username.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the specified product is not found in the basket.</exception>
    public async Task<DeleteBasketItemCommandResult> Handle(
        DeleteBasketItemCommand request,
        CancellationToken cancellationToken)
    {
        // Retrieve the user's basket
        var basket = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken);

        // Find the item to delete
        var itemToDelete = basket.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        
        if (itemToDelete == null)
        {
            throw new InvalidOperationException(
                $"Product with ID {request.ProductId} not found in basket for user {request.UserName}");
        }

        // Remove the item from the collection
        var updatedItems = basket.Items.Where(i => i.ProductId != request.ProductId).ToList();
        
        // Create updated basket with filtered items
        var updatedBasket = new ShoppingCart(basket.UserName)
        {
            Items = updatedItems
        };

        // Save the updated basket
        var savedBasket = await repository.CreateBasketAsync(updatedBasket, cancellationToken);

        return new DeleteBasketItemCommandResult(true, savedBasket.UserName, savedBasket);
    }
}

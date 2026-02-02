using Basket.API.Data.Repositories;
using Basket.API.Exceptions;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.UpdateItemQuantity;

/// <summary>
/// Handles the UpdateItemQuantityCommand by updating the quantity of a specific item in the user's basket.
/// </summary>
public class UpdateItemQuantityCommandHandler(IBasketRepository repository) 
    : ICommandHandler<UpdateItemQuantityCommand, UpdateItemQuantityCommandResult>
{
    /// <summary>
    /// Handles the request to update the quantity of a specific item in the shopping basket.
    /// </summary>
    /// <param name="request">The UpdateItemQuantityCommand containing the user, product, and new quantity.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task representing the asynchronous operation, returning an UpdateItemQuantityCommandResult.</returns>
    /// <exception cref="BasketNotFoundException">Thrown when no basket is found for the specified username.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the specified product is not found in the basket.</exception>
    public async Task<UpdateItemQuantityCommandResult> Handle(
        UpdateItemQuantityCommand request,
        CancellationToken cancellationToken)
    {
        // Retrieve the user's basket
        var basket = await repository.GetBasketByUserNameAsync(request.UserName, cancellationToken);

        // Find the item to update
        var itemToUpdate = basket.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        
        if (itemToUpdate == null)
        {
            throw new InvalidOperationException(
                $"Product with ID {request.ProductId} not found in basket for user {request.UserName}");
        }

        // Update the quantity
        itemToUpdate.Quantity = request.Quantity;

        // Save the updated basket
        var updatedBasket = await repository.CreateBasketAsync(basket, cancellationToken);

        return new UpdateItemQuantityCommandResult(true, updatedBasket.UserName, updatedBasket);
    }
}

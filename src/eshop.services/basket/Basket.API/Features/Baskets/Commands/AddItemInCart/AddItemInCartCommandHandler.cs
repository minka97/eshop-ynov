using Basket.API.Data.Repositories;
using Basket.API.Exceptions;
using Basket.API.Http;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddItemInCart;

/// <summary>
/// Handles the command to add an item to the shopping cart for a specified user.
/// This class retrieves the user's shopping cart from the repository, checks if the requested product exists
/// in the catalog, and then updates the cart with the new item before persisting the changes.
/// </summary>
/// <remarks>
/// This class implements the ICommandHandler interface for the AddItemInCartCommand and AddItemInCartCommandResult types.
/// It ensures that the product exists in the catalog before adding it to the user's shopping cart.
/// </remarks>
/// <param name="repository">The repository used to fetch and persist shopping cart data.</param>
/// <param name="catalogClient">A client used to communicate with the catalog service for validating product existence.</param>
public class AddItemInCartCommandHandler(IBasketRepository repository, CatalogClient catalogClient) : ICommandHandler<AddItemInCartCommand, AddItemInCartCommandResult>
{
    /// <summary>
    /// Handles the process of adding an item to the user's shopping cart.
    /// </summary>
    /// <param name="request">The command containing the username and the item to be added to the shopping cart.</param>
    /// <param name="cancellationToken">A token that propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="AddItemInCartCommandResult"/> containing the updated shopping cart.</returns>
    /// <exception cref="BasketNotFoundException">Thrown when the shopping cart for the specified user is not found.</exception>
    /// <exception cref="ProductNotFoundException">Thrown when the specified product does not exist in the catalog.</exception>
    public async Task<AddItemInCartCommandResult> Handle(AddItemInCartCommand request,
        CancellationToken cancellationToken)
    {
        var shoppingCart = await repository.GetBasketByUserNameAsync(request.Username, cancellationToken);

        if (shoppingCart is null)
        {
            throw new BasketNotFoundException(request.Username);
        }

        var shoppingCartItem = await catalogClient.GetProductById(request.Item.ProductId, cancellationToken);

        if (shoppingCartItem is null)
        {
            throw new ProductNotFoundException(request.Item.ProductId.ToString());
        }
        
        var tempList = shoppingCart.Items.ToList();
        shoppingCartItem.Color = request.Item.Color;
        shoppingCartItem.Quantity = request.Item.Quantity;
        tempList.Add(shoppingCartItem);
        
        shoppingCart.Items = tempList;
        
        var updatedShoppingCart = await repository.UpdateBasketAsync(shoppingCart, cancellationToken);

        return new AddItemInCartCommandResult(updatedShoppingCart);
    }
}
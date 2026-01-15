using Basket.API.Models;
using BuildingBlocks.CQRS;

namespace Basket.API.Features.Baskets.Commands.AddItemInCart;

/// <summary>
/// Represents a command to add an item to a user's shopping cart.
/// </summary>
/// <remarks>
/// This command encapsulates the data required to perform the operation of adding an item
/// to a shopping cart. It includes the username associated with the shopping cart and
/// the details of the item to be added.
/// </remarks>
/// <param name="Username">
/// The username of the user whose shopping cart is to be updated.
/// </param>
/// <param name="Item">
/// The item to be added to the user's shopping cart.
/// </param>
/// <returns>
/// An instance of <see cref="AddItemInCartCommandResult"/> containing the updated shopping cart.
/// </returns>
public record AddItemInCartCommand(string Username, ShoppingCartItem Item) : ICommand<AddItemInCartCommandResult>;
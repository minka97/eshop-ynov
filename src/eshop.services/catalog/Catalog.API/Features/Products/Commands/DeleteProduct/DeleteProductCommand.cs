using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Represents the command to delete a new product.
/// </summary>
public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductCommandResult>;
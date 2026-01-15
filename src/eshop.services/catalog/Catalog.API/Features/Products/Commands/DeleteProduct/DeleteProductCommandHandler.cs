using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Handles the CreateProduct command to create a new product in the system by persisting it through the provided document session.
/// </summary>
public class DeleteProductCommandHandler(IDocumentSession documentSession): ICommandHandler<DeleteProductCommand, DeleteProductCommandResult>
{
    /// <summary>
    /// Handles the processing of the DeleteProduct command, which removes an existing product from the system.
    /// It ensures the product exists before deleting it from the database.
    /// </summary>
    /// <param name="request">The DeleteProduct command containing the identifier of the product to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the operation, containing the result of the command which indicates whether the deletion succeeded.
    /// </returns>
    /// <exception cref="ProductNotFoundException">
    /// Thrown when the product with the specified identifier does not exist in the system.
    /// </exception>
    public async Task<DeleteProductCommandResult> Handle(DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await documentSession.LoadAsync<Product>(request.Id, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(request.Id);
        }

        documentSession.Delete(product);
        await documentSession.SaveChangesAsync(cancellationToken);
        
        return new DeleteProductCommandResult(true);
    }
}
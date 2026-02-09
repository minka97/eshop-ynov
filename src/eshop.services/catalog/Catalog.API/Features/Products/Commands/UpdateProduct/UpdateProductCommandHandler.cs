using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Handles the UpdateProductCommand to update an existing product in the system.
/// </summary>
public class UpdateProductCommandHandler(IDocumentSession documentSession)
    : ICommandHandler<UpdateProductCommand, UpdateProductCommandResult>
{
    /// <summary>
    /// Handles the processing of the UpdateProduct command.
    /// </summary>
    /// <param name="request">The UpdateProduct command containing the updated product details.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task containing the result indicating whether the update was successful.</returns>
    /// <exception cref="ProductNotFoundException">Thrown when the product to update is not found.</exception>
    public async Task<UpdateProductCommandResult> Handle(UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Charger le produit existant
        var product = await documentSession.LoadAsync<Product>(request.Id, cancellationToken);

        // 2. Vérifier qu'il existe
        if (product is null)
            throw new ProductNotFoundException(request.Id);

        // 3. Mettre à jour les propriétés
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.ImageFile = request.ImageFile;
        product.Categories = request.Categories;

        // 4. Sauvegarder les modifications
        documentSession.Update(product);
        await documentSession.SaveChangesAsync(cancellationToken);

        return new UpdateProductCommandResult(true);
    }
}
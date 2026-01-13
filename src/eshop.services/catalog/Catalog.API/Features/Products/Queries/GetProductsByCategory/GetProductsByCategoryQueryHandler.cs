using BuildingBlocks.CQRS;
using Catalog.API.Exceptions;
using Catalog.API.Models;
using ImTools;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// Handles the execution of the <see cref="GetProductByIdQuery"/> and retrieves the corresponding
/// product data from the data store.
/// </summary>
/// <remarks>
/// This class interacts with the database session to load the product identified by
/// the specified <see cref="Guid"/> in the query. If the product does not exist,
/// a <see cref="ProductNotFoundException"/> is thrown. It utilizes a logger to log
/// the status and outcome of the operation.
/// </remarks>
public class GetProductsByCategoryQueryHandler(IDocumentSession documentSession) 
    : IQueryHandler<GetProductsByCategoryQuery, GetProductsByCategoryQueryResult>
{
    /// <summary>
    /// Handles the execution of the GetProductsByCategoryQuery and retrieves the associated product data.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ProductsByCategoryNotFoundException"></exception>
    public async Task<GetProductsByCategoryQueryResult> Handle(GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var products = await documentSession.Query<Product>()
            .Where(x => x.Categories.Contains(request.Category))
            .ToListAsync(cancellationToken);
        
        if (products is null || !products.Any())
        {
            throw new ProductsByCategoryNotFoundException(request.Category);
        }
        
        return new GetProductsByCategoryQueryResult(products);
    }
}
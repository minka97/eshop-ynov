using BuildingBlocks.CQRS;
using Catalog.API.Models;
using Marten;

namespace Catalog.API.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IDocumentSession documentSession) 
    : IQueryHandler<GetProductsQuery, GetProductsQueryResult>
{
    public async Task<GetProductsQueryResult> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Récupérer le nombre total de produits
        var totalCount = await documentSession
            .Query<Product>()
            .CountAsync(cancellationToken);
        
        var products = await documentSession
            .Query<Product>()
            .Skip((request.NumPage - 1) * request.SizePage)
            .Take(request.SizePage)
            .ToListAsync(cancellationToken);
        
        return new GetProductsQueryResult(
            products, 
            totalCount, 
            request.NumPage, 
            request.SizePage
        );
    }
}

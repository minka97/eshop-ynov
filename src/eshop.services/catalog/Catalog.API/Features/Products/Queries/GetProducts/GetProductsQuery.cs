using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProducts;

public record GetProductsQuery(int NumPage, int SizePage) 
    : IQuery<GetProductsQueryResult>;

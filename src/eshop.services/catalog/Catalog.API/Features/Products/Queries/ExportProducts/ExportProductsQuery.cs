using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

public record ExportProductsQuery(int PageNumber = 1, int PageSize = 100)
    : IQuery<ExportProductsQueryResult>;

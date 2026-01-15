using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProducts;

public record GetProductsQueryResult(
    IEnumerable<Product> Products,
    int TotalCount,
    int NumPage,
    int SizePage
);

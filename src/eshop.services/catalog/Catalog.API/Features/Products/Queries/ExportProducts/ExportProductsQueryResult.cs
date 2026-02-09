namespace Catalog.API.Features.Products.Queries.ExportProducts;

public record ExportProductsQueryResult(byte[] FileContent, string FileName);

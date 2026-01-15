namespace Catalog.API.Features.Products.Commands.ImportProducts;

public record ImportProductsCommandResult(int Created, int Updated, List<string> Errors);

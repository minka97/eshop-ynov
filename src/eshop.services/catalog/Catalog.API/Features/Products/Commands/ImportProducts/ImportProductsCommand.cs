using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Http;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

public record ImportProductsCommand(IFormFile File) : ICommand<ImportProductsCommandResult>;

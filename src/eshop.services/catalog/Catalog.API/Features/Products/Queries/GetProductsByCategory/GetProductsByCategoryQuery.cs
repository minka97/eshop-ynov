using BuildingBlocks.CQRS;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;

/// <summary>
/// 
/// </summary>
/// <param name="Category"></param>
public record GetProductsByCategoryQuery(string Category) : IQuery<GetProductsByCategoryQueryResult>;
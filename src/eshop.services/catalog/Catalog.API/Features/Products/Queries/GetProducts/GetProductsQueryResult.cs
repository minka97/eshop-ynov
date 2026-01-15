using Catalog.API.Models;

namespace Catalog.API.Features.Products.Queries.GetProducts;

/// <summary>
/// Résultat de la requête GetProducts avec les informations de pagination.
/// </summary>
/// <param name="Products">Liste des produits de la page courante</param>
/// <param name="TotalCount">Nombre total de produits dans la base</param>
/// <param name="PageNumber">Numéro de la page courante</param>
/// <param name="PageSize">Nombre de produits par page</param>
public record GetProductsQueryResult(
    IEnumerable<Product> Products,
    int TotalCount,
    int PageNumber,
    int PageSize
);

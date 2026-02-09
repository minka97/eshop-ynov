using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

/// <summary>
/// Query pour rechercher des commandes par nom
/// Pattern CQRS : Recherche textuelle
/// </summary>
/// <param name="Name">Le nom ou partie du nom de commande à rechercher</param>
public record GetOrdersByNameQuery(string Name) 
    : IQuery<GetOrdersByNameResult>;

using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

/// <summary>
/// Query pour récupérer toutes les commandes avec pagination
/// Pattern CQRS : Séparation lecture/écriture
/// </summary>
/// <param name="PageIndex">Index de la page (0-based, commence à 0)</param>
/// <param name="PageSize">Nombre d'éléments par page (ex: 10)</param>
public record GetOrdersQuery(int PageIndex = 0, int PageSize = 10) 
    : IQuery<GetOrdersResult>;

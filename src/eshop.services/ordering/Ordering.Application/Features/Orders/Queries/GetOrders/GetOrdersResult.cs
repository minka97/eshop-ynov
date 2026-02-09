using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

/// <summary>
/// Résultat de la query GetOrders
/// Retourne une liste paginée de commandes
/// </summary>
/// <param name="Orders">Liste des DTOs de commandes</param>
public record GetOrdersResult(IEnumerable<OrderDto> Orders);

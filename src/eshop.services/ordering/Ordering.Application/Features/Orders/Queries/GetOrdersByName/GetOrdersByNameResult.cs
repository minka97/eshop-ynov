using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

/// <summary>
/// Résultat de la query GetOrdersByName
/// Retourne toutes les commandes correspondant au nom recherché
/// </summary>
/// <param name="Orders">Liste des DTOs de commandes trouvées</param>
public record GetOrdersByNameResult(IEnumerable<OrderDto> Orders);

using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Résultat de la query GetOrderById
/// Retourne null si la commande n'existe pas
/// </summary>
/// <param name="Order">Le DTO de la commande ou null si non trouvée</param>
public record GetOrderByIdResult(OrderDto? Order);

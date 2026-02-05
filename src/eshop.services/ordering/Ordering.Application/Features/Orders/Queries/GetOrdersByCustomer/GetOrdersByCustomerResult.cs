using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;

/// <summary>
/// Résultat de la query GetOrdersByCustomer
/// Retourne toutes les commandes d'un client
/// </summary>
/// <param name="Orders">Liste des DTOs de commandes du client</param>
public record GetOrdersByCustomerResult(IEnumerable<OrderDto> Orders);

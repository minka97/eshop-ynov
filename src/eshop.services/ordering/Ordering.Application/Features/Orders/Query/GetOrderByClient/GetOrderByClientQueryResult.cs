using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Query.GetOrderByClient;

/// <summary>
/// Represents the result of a query to retrieve orders by client name.
/// </summary>
/// <param name="Orders">The collection of orders for the specified client.</param>
public record GetOrderByClientQueryResult(IEnumerable<OrderDto> Orders);

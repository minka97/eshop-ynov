using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Query.GetAllOrder;

/// <summary>
/// Represents the result of a query to retrieve all orders.
/// </summary>
/// <param name="Orders">The collection of orders retrieved from the system.</param>
public record GetAllOrderQueryResult(IEnumerable<OrderDto> Orders);

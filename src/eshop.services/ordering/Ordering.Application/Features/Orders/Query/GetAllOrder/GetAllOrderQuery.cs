using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Query.GetAllOrder;

/// <summary>
/// Represents a query to retrieve all orders from the ordering system with pagination support.
/// </summary>
/// <param name="PageIndex">The zero-based index of the page to retrieve.</param>
/// <param name="PageSize">The number of orders to include in each page of results.</param>
public record GetAllOrderQuery(int PageIndex = 0, int PageSize = 10) : IQuery<GetAllOrderQueryResult>;

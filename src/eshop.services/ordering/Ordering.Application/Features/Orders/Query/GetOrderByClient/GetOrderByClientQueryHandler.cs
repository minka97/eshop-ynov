using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Query.GetOrderByClient;

/// <summary>
/// Handles the query to retrieve orders for a specific client by their name.
/// </summary>
public class GetOrderByClientQueryHandler(IOrderingDbContext orderingDbContext)
    : IQueryHandler<GetOrderByClientQuery, GetOrderByClientQueryResult>
{
    /// <summary>
    /// Handles the retrieval of orders for a client by their name.
    /// </summary>
    /// <param name="request">The query request containing the client name.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A result containing the orders for the specified client.</returns>
    public async Task<GetOrderByClientQueryResult> Handle(GetOrderByClientQuery request, CancellationToken cancellationToken)
    {
        var customer = await orderingDbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.Contains(request.ClientName), cancellationToken);

        if (customer is null)
        {
            return new GetOrderByClientQueryResult(Enumerable.Empty<OrderDto>());
        }

        var orders = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .Where(o => o.CustomerId == customer.Id)
            .ToListAsync(cancellationToken);

        var orderDtos = orders.ToOrderDtoList();

        return new GetOrderByClientQueryResult(orderDtos);
    }
}

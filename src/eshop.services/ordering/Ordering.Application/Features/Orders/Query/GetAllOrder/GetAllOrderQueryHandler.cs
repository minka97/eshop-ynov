using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Extensions;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Query.GetAllOrder;

/// <summary>
/// Handles the query to retrieve all orders from the database.
/// </summary>
public class GetAllOrderQueryHandler(IOrderingDbContext orderingDbContext)
    : IQueryHandler<GetAllOrderQuery, GetAllOrderQueryResult>
{
    /// <summary>
    /// Handles the retrieval of all orders from the database with pagination.
    /// </summary>
    /// <param name="request">The query request containing pagination parameters.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A result containing paginated orders.</returns>
    public async Task<GetAllOrderQueryResult> Handle(GetAllOrderQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderingDbContext.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var orderDtos = orders.ToOrderDtoList();

        return new GetAllOrderQueryResult(orderDtos);
    }
}

using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Query.GetOrderByClient;

/// <summary>
/// Represents a query to retrieve orders for a specific client by their name.
/// </summary>
/// <param name="ClientName">The name of the client to search for.</param>
public record GetOrderByClientQuery(string ClientName) : IQuery<GetOrderByClientQueryResult>;

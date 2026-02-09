using BuildingBlocks.CQRS;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;

/// <summary>
/// Query pour récupérer toutes les commandes d'un client spécifique
/// Pattern CQRS : Lecture par filtre CustomerId
/// </summary>
/// <param name="CustomerId">L'ID du client dont on veut les commandes</param>
public record GetOrdersByCustomerQuery(Guid CustomerId) 
    : IQuery<GetOrdersByCustomerResult>;

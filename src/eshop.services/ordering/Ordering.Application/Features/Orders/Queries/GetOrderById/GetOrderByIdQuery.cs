using BuildingBlocks.CQRS;
using Ordering.Application.Features.Orders.Dtos;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Query pour récupérer une commande par son ID
/// Pattern CQRS : Lecture seule, pas de modification
/// </summary>
/// <param name="OrderId">L'ID de la commande à récupérer</param>
public record GetOrderByIdQuery(Guid OrderId) : IQuery<GetOrderByIdResult>;

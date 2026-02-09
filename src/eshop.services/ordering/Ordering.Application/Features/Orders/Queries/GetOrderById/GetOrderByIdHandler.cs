using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Handler pour récupérer une commande par ID
/// Respecte Clean Architecture : utilise IOrderingDbContext (abstraction)
/// Pattern CQRS : Query Handler (lecture seule)
/// </summary>
public class GetOrderByIdHandler(IOrderingDbContext dbContext) 
    : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    public async Task<GetOrderByIdResult> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Étape 1 : Récupérer Order depuis la base (Aggregate complet)
        // On utilise AsNoTracking car c'est une lecture seule (optimisation)
        var order = await dbContext.Orders
            .Include(o => o.OrderItems) // Eager loading des items (relation 1-N)
            .AsNoTracking() // Pas de tracking EF Core pour améliorer les performances
            .FirstOrDefaultAsync(o => o.Id.Value == request.OrderId, cancellationToken);

        // Étape 2 : Si pas trouvé, retourner null
        if (order == null)
        {
            return new GetOrderByIdResult(null);
        }

        // Étape 3 : Mapper Domain Entity → DTO (séparation des couches)
        var orderDto = MapOrderToDto(order);

        // Étape 4 : Retourner le résultat
        return new GetOrderByIdResult(orderDto);
    }

    /// <summary>
    /// Mapping manuel Order (Domain) → OrderDto (Application)
    /// Respecte la séparation des couches (Domain ne doit pas connaître Application)
    /// Extrait les valeurs des Value Objects (ex: OrderId.Value)
    /// </summary>
    private static OrderDto MapOrderToDto(Order order)
    {
        return new OrderDto(
            Id: order.Id.Value, // Value Object → Guid primitif
            CustomerId: order.CustomerId.Value,
            OrderName: order.OrderName.Value,
            ShippingAddress: new AddressDto(
                order.ShippingAddress.FirstName,
                order.ShippingAddress.LastName,
                order.ShippingAddress.EmailAddress!,
                order.ShippingAddress.AddressLine,
                order.ShippingAddress.Country,
                order.ShippingAddress.State,
                order.ShippingAddress.ZipCode
            ),
            BillingAddress: new AddressDto(
                order.BillingAddress.FirstName,
                order.BillingAddress.LastName,
                order.BillingAddress.EmailAddress!,
                order.BillingAddress.AddressLine,
                order.BillingAddress.Country,
                order.BillingAddress.State,
                order.BillingAddress.ZipCode
            ),
            Payment: new PaymentDto(
                order.Payment.CardName!,
                order.Payment.CardNumber,
                order.Payment.Expiration,
                order.Payment.CVV,
                order.Payment.PaymentMethod
            ),
            OrderStatus: order.OrderStatus,
            OrderItems: order.OrderItems.Select(item => new OrderItemDto(
                item.OrderId.Value,
                item.ProductId.Value,
                item.Quantity,
                item.Price
            )).ToList()
        );
    }
}

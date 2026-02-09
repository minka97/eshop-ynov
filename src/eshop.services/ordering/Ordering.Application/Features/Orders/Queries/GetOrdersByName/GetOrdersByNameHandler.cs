using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByName;

/// <summary>
/// Handler pour rechercher des commandes par nom
/// Utilise Contains pour recherche partielle (like %name%)
/// </summary>
public class GetOrdersByNameHandler(IOrderingDbContext dbContext) 
    : IQueryHandler<GetOrdersByNameQuery, GetOrdersByNameResult>
{
    public async Task<GetOrdersByNameResult> Handle(
        GetOrdersByNameQuery request,
        CancellationToken cancellationToken)
    {
        // Étape 1 : Rechercher par nom (contient)
        // IMPORTANT : OrderName est un Value Object, on compare avec .Value
        var orders = await dbContext.Orders
            .Include(o => o.OrderItems) // Charger les items
            .AsNoTracking() // Pas de tracking pour lecture
            .Where(o => o.OrderName.Value.Contains(request.Name)) // Recherche partielle
            .OrderByDescending(o => o.CreatedAt) // Plus récentes en premier
            .ToListAsync(cancellationToken);

        // Étape 2 : Mapper vers DTOs
        var orderDtos = orders.Select(MapOrderToDto).ToList();

        // Étape 3 : Retourner le résultat
        return new GetOrdersByNameResult(orderDtos);
    }

    /// <summary>
    /// Mapping manuel Order (Domain) → OrderDto (Application)
    /// </summary>
    private static OrderDto MapOrderToDto(Order order)
    {
        return new OrderDto(
            Id: order.Id.Value,
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

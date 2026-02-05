using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Queries.GetOrdersByCustomer;

/// <summary>
/// Handler pour récupérer toutes les commandes d'un client
/// Filtre par CustomerId (Value Object)
/// </summary>
public class GetOrdersByCustomerHandler(IOrderingDbContext dbContext) 
    : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    public async Task<GetOrdersByCustomerResult> Handle(
        GetOrdersByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        // Étape 1 : Filtrer par CustomerId
        // IMPORTANT : On compare avec .Value car CustomerId est un Value Object
        var orders = await dbContext.Orders
            .Include(o => o.OrderItems) // Charger les items
            .AsNoTracking() // Pas de tracking pour lecture
            .Where(o => o.CustomerId.Value == request.CustomerId) // Filtre par client
            .OrderByDescending(o => o.CreatedAt) // Plus récentes en premier
            .ToListAsync(cancellationToken);

        // Étape 2 : Mapper vers DTOs
        var orderDtos = orders.Select(MapOrderToDto).ToList();

        // Étape 3 : Retourner le résultat
        return new GetOrdersByCustomerResult(orderDtos);
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

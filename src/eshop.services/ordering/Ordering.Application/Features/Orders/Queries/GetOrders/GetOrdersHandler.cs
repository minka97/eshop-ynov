using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Models;

namespace Ordering.Application.Features.Orders.Queries.GetOrders;

/// <summary>
/// Handler pour récupérer toutes les commandes avec pagination
/// Respecte Clean Architecture et CQRS
/// </summary>
public class GetOrdersHandler(IOrderingDbContext dbContext) 
    : IQueryHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        // Étape 1 : Calculer la pagination (Skip/Take pattern)
        int skip = request.PageIndex * request.PageSize;
        int take = request.PageSize;

        // Étape 2 : Récupérer les commandes (paginées et triées)
        var orders = await dbContext.Orders
            .Include(o => o.OrderItems) // Charger les items de chaque commande
            .AsNoTracking() // Optimisation : pas de tracking pour lecture
            .OrderByDescending(o => o.CreatedAt) // Plus récentes en premier
            .Skip(skip) // Sauter les N premières
            .Take(take) // Prendre seulement PageSize éléments
            .ToListAsync(cancellationToken);

        // Étape 3 : Mapper chaque Order → OrderDto
        var orderDtos = orders.Select(MapOrderToDto).ToList();

        // Étape 4 : Retourner le résultat
        return new GetOrdersResult(orderDtos);
    }

    /// <summary>
    /// Mapping manuel Order (Domain) → OrderDto (Application)
    /// Extrait les valeurs des Value Objects
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

using BuildingBlocks.CQRS;
using Ordering.Domain.Enums;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) 
    : ICommand<UpdateOrderStatusResult>;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Features.Orders.Data;

namespace Ordering.Application.Features.Orders.Commands.UpdateOrderStatus;

/// <summary>
/// Gère la logique pour mettre à jour le statut d'une commande
/// </summary>
public class UpdateOrderStatusHandler(IOrderingDbContext dbContext) 
    : ICommandHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    public async Task<UpdateOrderStatusResult> Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        // Étape 1 : Chercher la commande dans la base de données
        // On compare avec o.Id.Value car Id est un ValueObject OrderId
        var order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id.Value == request.OrderId, cancellationToken);

        // Étape 2 : Vérifier que la commande existe
        if (order == null)
        {
            // La commande n'existe pas, on retourne false
            return new UpdateOrderStatusResult(false);
        }

        // Étape 3 : Changer le statut
        order.OrderStatus = request.NewStatus;

        // Étape 4 : Sauvegarder les modifications
        await dbContext.SaveChangesAsync(cancellationToken);

        // Étape 5 : Retourner le succès
        return new UpdateOrderStatusResult(true);
    }
}
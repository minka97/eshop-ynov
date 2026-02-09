using Email.API.Events;
using Email.API.Services;
using MassTransit;

namespace Email.API.EventHandlers;

/// <summary>
/// Consumer pour l'événement OrderUpdatedEvent
/// Écoute RabbitMQ et envoie un email au client lors du changement de statut de commande
/// </summary>
public class OrderUpdatedEventConsumer : IConsumer<OrderUpdatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderUpdatedEventConsumer> _logger;

    public OrderUpdatedEventConsumer(IEmailService emailService, ILogger<OrderUpdatedEventConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderUpdatedEvent> context)
    {
        var orderEvent = context.Message;
        var order = orderEvent.Order;
        
        _logger.LogInformation("🔄 OrderUpdatedEvent reçu via RabbitMQ : {OrderId}, Statut: {Status}", 
            order.Id.Value, order.OrderStatus);

        try
        {
            // Construire l'email de notification de changement de statut
            var emailBody = BuildOrderStatusChangeEmail(orderEvent);
            var subject = GetEmailSubjectForStatus(order.OrderStatus, order.OrderName.Value);

            // Envoyer l'email au client
            var emailSent = await _emailService.SendEmailAsync(
                to: order.ShippingAddress.EmailAddress ?? "noemail@example.com",
                subject: subject,
                body: emailBody,
                cancellationToken: context.CancellationToken
            );

            if (emailSent)
            {
                _logger.LogInformation("✅ Email de changement de statut envoyé à {Email}", 
                    order.ShippingAddress.EmailAddress);
            }
            else
            {
                _logger.LogWarning("⚠️ Échec de l'envoi d'email pour la commande {OrderId}", 
                    order.Id.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de l'envoi d'email pour la commande {OrderId}", 
                order.Id.Value);
        }
    }

    /// <summary>
    /// Construit le sujet de l'email en fonction du statut
    /// </summary>
    private static string GetEmailSubjectForStatus(Ordering.Domain.Enums.OrderStatus status, string orderName)
    {
        return status switch
        {
            Ordering.Domain.Enums.OrderStatus.Confirmed => $"✅ Votre commande {orderName} a été confirmée",
            Ordering.Domain.Enums.OrderStatus.Shipped => $"🚚 Votre commande {orderName} a été expédiée",
            Ordering.Domain.Enums.OrderStatus.Delivered => $"📦 Votre commande {orderName} a été livrée",
            Ordering.Domain.Enums.OrderStatus.Cancelled => $"❌ Votre commande {orderName} a été annulée",
            Ordering.Domain.Enums.OrderStatus.Completed => $"✅ Votre commande {orderName} est terminée",
            _ => $"🔄 Mise à jour de votre commande {orderName}"
        };
    }

    /// <summary>
    /// Construit le template HTML de l'email de changement de statut
    /// </summary>
    private static string BuildOrderStatusChangeEmail(OrderUpdatedEvent orderEvent)
    {
        var order = orderEvent.Order;
        var status = order.OrderStatus;
        
        // Calculer le total
        var total = order.OrderItems.Sum(item => item.Price * item.Quantity);

        // Obtenir le message et la couleur selon le statut
        var (statusMessage, statusColor, statusIcon) = GetStatusMessageAndColor(status);

        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {statusColor}; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .status-box {{ background-color: {statusColor}; color: white; padding: 15px; margin: 15px 0; border-radius: 5px; text-align: center; font-size: 1.2em; font-weight: bold; }}
        .order-details {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .item {{ border-bottom: 1px solid #eee; padding: 10px 0; }}
        .item:last-child {{ border-bottom: none; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 0.9em; color: #666; padding: 20px; background-color: #f0f0f0; border-radius: 0 0 5px 5px; }}
        h1 {{ margin: 0; }}
        h3 {{ color: {statusColor}; margin-top: 0; }}
        .tracking-info {{ background-color: #e3f2fd; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #2196F3; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{statusIcon} Mise à jour de votre commande</h1>
        </div>
        
        <div class=""content"">
            <p>Bonjour <strong>{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}</strong>,</p>
            
            <div class=""status-box"">
                {statusMessage}
            </div>
            
            {GetStatusSpecificContent(status, order)}
            
            <div class=""order-details"">
                <h3>📦 Détails de la commande</h3>
                <p><strong>Numéro de commande :</strong> {order.OrderName.Value}</p>
                <p><strong>Statut actuel :</strong> {GetStatusLabel(status)}</p>
                <p><strong>Total :</strong> {total:C2} €</p>
            </div>
            
            <div class=""order-details"">
                <h3>🚚 Adresse de livraison</h3>
                <p>
                    {order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}<br>
                    {order.ShippingAddress.AddressLine}<br>
                    {order.ShippingAddress.ZipCode} {order.ShippingAddress.State}<br>
                    {order.ShippingAddress.Country}
                </p>
            </div>
        </div>
        
        <div class=""footer"">
            <p><strong>Merci d'avoir choisi eShop YNOV !</strong></p>
            <p style=""font-size: 0.8em; margin-top: 10px;"">Ceci est un email automatique, merci de ne pas y répondre.</p>
        </div>
    </div>
</body>
</html>";

        return emailBody;
    }

    /// <summary>
    /// Retourne le message, la couleur et l'icône selon le statut
    /// </summary>
    private static (string message, string color, string icon) GetStatusMessageAndColor(Ordering.Domain.Enums.OrderStatus status)
    {
        return status switch
        {
            Ordering.Domain.Enums.OrderStatus.Confirmed => 
                ("Votre commande a été confirmée et est en cours de préparation !", "#4CAF50", "✅"),
            
            Ordering.Domain.Enums.OrderStatus.Shipped => 
                ("Votre commande a été expédiée et est en route vers vous !", "#2196F3", "🚚"),
            
            Ordering.Domain.Enums.OrderStatus.Delivered => 
                ("Votre commande a été livrée avec succès !", "#4CAF50", "📦"),
            
            Ordering.Domain.Enums.OrderStatus.Cancelled => 
                ("Votre commande a été annulée", "#f44336", "❌"),
            
            Ordering.Domain.Enums.OrderStatus.Completed => 
                ("Votre commande est terminée. Merci pour votre achat !", "#4CAF50", "✅"),
            
            _ => ("Votre commande a été mise à jour", "#FF9800", "🔄")
        };
    }

    /// <summary>
    /// Retourne du contenu spécifique selon le statut (numéro de suivi, etc.)
    /// </summary>
    private static string GetStatusSpecificContent(Ordering.Domain.Enums.OrderStatus status, Ordering.Domain.Models.Order order)
    {
        return status switch
        {
            Ordering.Domain.Enums.OrderStatus.Shipped => @"
                <div class=""tracking-info"">
                    <h4 style=""margin-top: 0; color: #2196F3;"">📍 Informations de suivi</h4>
                    <p>Votre colis est en cours de livraison. Vous pouvez suivre votre commande en temps réel.</p>
                    <p><strong>Délai de livraison estimé :</strong> 2-3 jours ouvrables</p>
                    <p style=""margin-bottom: 0;""><em>Vous recevrez une notification dès que votre colis sera livré.</em></p>
                </div>",
            
            Ordering.Domain.Enums.OrderStatus.Delivered => @"
                <div class=""tracking-info"">
                    <h4 style=""margin-top: 0; color: #4CAF50;"">📦 Livraison effectuée</h4>
                    <p>Votre commande a été livrée à l'adresse indiquée.</p>
                    <p><em>Nous espérons que vous êtes satisfait de votre achat. N'hésitez pas à nous laisser un avis !</em></p>
                </div>",
            
            Ordering.Domain.Enums.OrderStatus.Cancelled => @"
                <div class=""tracking-info"" style=""background-color: #ffebee; border-left-color: #f44336;"">
                    <h4 style=""margin-top: 0; color: #f44336;"">❌ Commande annulée</h4>
                    <p>Votre commande a été annulée. Si vous n'êtes pas à l'origine de cette annulation, veuillez nous contacter.</p>
                    <p><em>Le remboursement sera effectué dans les 5-10 jours ouvrables.</em></p>
                </div>",
            
            _ => ""
        };
    }

    /// <summary>
    /// Convertit le statut en libellé français
    /// </summary>
    private static string GetStatusLabel(Ordering.Domain.Enums.OrderStatus status)
    {
        return status switch
        {
            Ordering.Domain.Enums.OrderStatus.Draft => "📝 Brouillon",
            Ordering.Domain.Enums.OrderStatus.Pending => "⏳ En attente",
            Ordering.Domain.Enums.OrderStatus.Submitted => "📤 Soumise",
            Ordering.Domain.Enums.OrderStatus.Cancelled => "❌ Annulée",
            Ordering.Domain.Enums.OrderStatus.Confirmed => "✅ Confirmée",
            Ordering.Domain.Enums.OrderStatus.Completed => "✅ Terminée",
            Ordering.Domain.Enums.OrderStatus.Shipped => "🚚 Expédiée",
            Ordering.Domain.Enums.OrderStatus.Delivered => "📦 Livrée",
            _ => status.ToString()
        };
    }
}

using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Services;
using Ordering.Domain.Events;

namespace Ordering.Application.Features.Orders.EventHandlers.Integration;

/// <summary>
/// Consumer pour OrderCreatedEvent (événement domaine)
/// Envoie un email de confirmation au client
/// Pattern Event-Driven : Découplage entre services
/// </summary>
public class OrderCreatedEventHandler : IConsumer<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        IEmailService emailService, 
        ILogger<OrderCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var orderEvent = context.Message;
        
        _logger.LogInformation("🎉 Order Created Event reçu : {OrderId}", orderEvent.Order.Id.Value);

        try
        {
            // Construire le contenu de l'email
            var emailBody = BuildOrderConfirmationEmail(orderEvent);

            // Envoyer l'email au client
            var emailSent = await _emailService.SendEmailAsync(
                to: orderEvent.Order.ShippingAddress.EmailAddress ?? "noemail@example.com",
                subject: $"Confirmation de votre commande {orderEvent.Order.OrderName.Value}",
                body: emailBody
            );

            if (emailSent)
            {
                _logger.LogInformation("✅ Email de confirmation envoyé à {Email}", 
                    orderEvent.Order.ShippingAddress.EmailAddress);
            }
            else
            {
                _logger.LogWarning("⚠️ Échec de l'envoi d'email pour la commande {OrderId}", 
                    orderEvent.Order.Id.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de l'envoi d'email pour la commande {OrderId}", 
                orderEvent.Order.Id.Value);
            
            // Ne pas lancer d'exception pour ne pas bloquer le traitement
            // L'email est une fonctionnalité "nice to have", pas critique
        }
    }

    /// <summary>
    /// Construit le contenu HTML de l'email de confirmation
    /// </summary>
    private static string BuildOrderConfirmationEmail(OrderCreatedEvent orderEvent)
    {
        var order = orderEvent.Order;
        
        // Calculer le total (somme des items)
        var total = order.OrderItems.Sum(item => item.Price * item.Quantity);

        // Template HTML simple
        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .order-details {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .item {{ border-bottom: 1px solid #eee; padding: 10px 0; }}
        .item:last-child {{ border-bottom: none; }}
        .total {{ font-size: 1.2em; font-weight: bold; color: #4CAF50; text-align: right; margin-top: 15px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 0.9em; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✅ Commande Confirmée</h1>
        </div>
        
        <div class=""content"">
            <p>Bonjour <strong>{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}</strong>,</p>
            
            <p>Votre commande a été créée avec succès ! Merci pour votre confiance.</p>
            
            <div class=""order-details"">
                <h3>📦 Détails de la commande</h3>
                <p><strong>Numéro de commande :</strong> {order.OrderName.Value}</p>
                <p><strong>Statut :</strong> {GetStatusLabel(order.OrderStatus)}</p>
                
                <h4>Produits commandés :</h4>
                {BuildItemsList(order.OrderItems)}
                
                <div class=""total"">
                    Total : {total:C2} €
                </div>
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
            
            <div class=""order-details"">
                <h3>💳 Informations de paiement</h3>
                <p>
                    <strong>Carte :</strong> {order.Payment.CardName}<br>
                    <strong>Numéro :</strong> **** **** **** {order.Payment.CardNumber.Substring(Math.Max(0, order.Payment.CardNumber.Length - 4))}<br>
                    <strong>Mode :</strong> {GetPaymentMethodLabel(order.Payment.PaymentMethod)}
                </p>
            </div>
            
            <p style=""margin-top: 20px;"">
                Vous recevrez un nouvel email lorsque votre commande sera expédiée.
            </p>
        </div>
        
        <div class=""footer"">
            <p>Merci d'avoir choisi eShop !</p>
            <p style=""font-size: 0.8em;"">Ceci est un email automatique, merci de ne pas y répondre.</p>
        </div>
    </div>
</body>
</html>";

        return emailBody;
    }

    /// <summary>
    /// Construit la liste HTML des items
    /// </summary>
    private static string BuildItemsList(IReadOnlyList<Ordering.Domain.Models.OrderItem> items)
    {
        var itemsHtml = "";
        foreach (var item in items)
        {
            itemsHtml += $@"
                <div class=""item"">
                    <strong>Produit ID:</strong> {item.ProductId.Value}<br>
                    <strong>Quantité:</strong> {item.Quantity}<br>
                    <strong>Prix unitaire:</strong> {item.Price:C2} €<br>
                    <strong>Sous-total:</strong> {(item.Price * item.Quantity):C2} €
                </div>";
        }
        return itemsHtml;
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

    /// <summary>
    /// Convertit le mode de paiement en libellé
    /// </summary>
    private static string GetPaymentMethodLabel(int paymentMethod)
    {
        return paymentMethod switch
        {
            1 => "Carte de crédit",
            2 => "PayPal",
            3 => "Virement bancaire",
            _ => "Autre"
        };
    }
}

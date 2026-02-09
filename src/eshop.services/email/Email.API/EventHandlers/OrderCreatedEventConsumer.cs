using Email.API.Events;
using Email.API.Services;
using MassTransit;

namespace Email.API.EventHandlers;

/// <summary>
/// Consumer pour l'événement OrderCreatedEvent
/// Écoute RabbitMQ et envoie un email de confirmation au client
/// Pattern Event-Driven : Email.API est découplé de Ordering.API
/// </summary>
public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventConsumer> _logger;

    public OrderCreatedEventConsumer(IEmailService emailService, ILogger<OrderCreatedEventConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var orderEvent = context.Message;
        
        _logger.LogInformation("🎉 OrderCreatedEvent reçu via RabbitMQ : {OrderId}", orderEvent.Order.Id.Value);

        try
        {
            // Construire le contenu HTML de l'email
            var emailBody = BuildOrderConfirmationEmail(orderEvent);

            // Envoyer l'email au client
            var emailSent = await _emailService.SendEmailAsync(
                to: orderEvent.Order.ShippingAddress.EmailAddress ?? "noemail@example.com",
                subject: $"✅ Confirmation de votre commande {orderEvent.Order.OrderName.Value}",
                body: emailBody,
                cancellationToken: context.CancellationToken
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
    /// Construit le template HTML de l'email de confirmation
    /// </summary>
    private static string BuildOrderConfirmationEmail(OrderCreatedEvent orderEvent)
    {
        var order = orderEvent.Order;
        
        // Calculer le total (somme des items)
        var total = order.OrderItems.Sum(item => item.Price * item.Quantity);

        // Template HTML professionnel
        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .order-details {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .item {{ border-bottom: 1px solid #eee; padding: 10px 0; }}
        .item:last-child {{ border-bottom: none; }}
        .total {{ font-size: 1.3em; font-weight: bold; color: #4CAF50; text-align: right; margin-top: 15px; padding-top: 15px; border-top: 2px solid #4CAF50; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 0.9em; color: #666; padding: 20px; background-color: #f0f0f0; border-radius: 0 0 5px 5px; }}
        h1 {{ margin: 0; }}
        h3 {{ color: #4CAF50; margin-top: 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✅ Commande Confirmée</h1>
            <p style=""margin: 5px 0 0 0;"">Merci pour votre achat !</p>
        </div>
        
        <div class=""content"">
            <p>Bonjour <strong>{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}</strong>,</p>
            
            <p>Votre commande a été créée avec succès ! Nous commençons à préparer votre colis.</p>
            
            <div class=""order-details"">
                <h3>📦 Détails de la commande</h3>
                <p><strong>Numéro de commande :</strong> {order.OrderName.Value}</p>
                <p><strong>Statut :</strong> {GetStatusLabel(order.OrderStatus)}</p>
                
                <h4 style=""margin-top: 20px; color: #666;"">Produits commandés :</h4>
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
            
            <p style=""margin-top: 20px; padding: 15px; background-color: #e8f5e9; border-left: 4px solid #4CAF50; border-radius: 3px;"">
                📧 Vous recevrez un nouvel email lorsque votre commande sera expédiée.
            </p>
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
    /// Construit la liste HTML des items de commande
    /// </summary>
    private static string BuildItemsList(IReadOnlyList<Ordering.Domain.Models.OrderItem> items)
    {
        var itemsHtml = "";
        foreach (var item in items)
        {
            itemsHtml += $@"
                <div class=""item"">
                    <strong>🛍️ Produit ID:</strong> {item.ProductId.Value}<br>
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
            1 => "💳 Carte de crédit",
            2 => "🅿️ PayPal",
            3 => "🏦 Virement bancaire",
            _ => "💰 Autre"
        };
    }
}

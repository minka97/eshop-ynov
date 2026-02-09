using Email.API.Services;
using MassTransit;
using Ordering.Application.Features.Orders.Dtos;
using Ordering.Domain.Enums;

namespace Email.API.EventHandlers;

/// <summary>
/// Consumer pour l'événement OrderDto publié par Ordering.API
/// Écoute RabbitMQ et envoie un email de confirmation au client
/// Pattern Event-Driven : Email.API est découplé de Ordering.API
/// </summary>
public class OrderCreatedEventConsumer : IConsumer<OrderDto>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventConsumer> _logger;

    public OrderCreatedEventConsumer(IEmailService emailService, ILogger<OrderCreatedEventConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderDto> context)
    {
        var order = context.Message;

        _logger.LogInformation("OrderDto recu via RabbitMQ : {OrderId}", order.Id);

        try
        {
            // Construire le contenu HTML de l'email
            var emailBody = BuildOrderConfirmationEmail(order);

            // Envoyer l'email au client
            var emailSent = await _emailService.SendEmailAsync(
                to: order.ShippingAddress.EmailAddress ?? "noemail@example.com",
                subject: $"Confirmation de votre commande {order.OrderName}",
                body: emailBody,
                cancellationToken: context.CancellationToken
            );

            if (emailSent)
            {
                _logger.LogInformation("Email de confirmation envoye a {Email}",
                    order.ShippingAddress.EmailAddress);
            }
            else
            {
                _logger.LogWarning("Echec de l'envoi d'email pour la commande {OrderId}",
                    order.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi d'email pour la commande {OrderId}",
                order.Id);
        }
    }

    /// <summary>
    /// Construit le template HTML de l'email de confirmation
    /// </summary>
    private static string BuildOrderConfirmationEmail(OrderDto order)
    {
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
            <h1>Commande Confirmee</h1>
            <p style=""margin: 5px 0 0 0;"">Merci pour votre achat !</p>
        </div>

        <div class=""content"">
            <p>Bonjour <strong>{order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}</strong>,</p>

            <p>Votre commande a ete creee avec succes ! Nous commencons a preparer votre colis.</p>

            <div class=""order-details"">
                <h3>Details de la commande</h3>
                <p><strong>Numero de commande :</strong> {order.OrderName}</p>
                <p><strong>Statut :</strong> {GetStatusLabel(order.OrderStatus)}</p>

                <h4 style=""margin-top: 20px; color: #666;"">Produits commandes :</h4>
                {BuildItemsList(order.OrderItems)}

                <div class=""total"">
                    Total : {total:C2}
                </div>
            </div>

            <div class=""order-details"">
                <h3>Adresse de livraison</h3>
                <p>
                    {order.ShippingAddress.FirstName} {order.ShippingAddress.LastName}<br>
                    {order.ShippingAddress.AddressLine}<br>
                    {order.ShippingAddress.ZipCode} {order.ShippingAddress.State}<br>
                    {order.ShippingAddress.Country}
                </p>
            </div>

            <div class=""order-details"">
                <h3>Informations de paiement</h3>
                <p>
                    <strong>Carte :</strong> {order.Payment.CardName}<br>
                    <strong>Numero :</strong> **** **** **** {order.Payment.CardNumber.Substring(Math.Max(0, order.Payment.CardNumber.Length - 4))}<br>
                    <strong>Mode :</strong> {GetPaymentMethodLabel(order.Payment.PaymentMethod)}
                </p>
            </div>

            <p style=""margin-top: 20px; padding: 15px; background-color: #e8f5e9; border-left: 4px solid #4CAF50; border-radius: 3px;"">
                Vous recevrez un nouvel email lorsque votre commande sera expediee.
            </p>
        </div>

        <div class=""footer"">
            <p><strong>Merci d'avoir choisi eShop YNOV !</strong></p>
            <p style=""font-size: 0.8em; margin-top: 10px;"">Ceci est un email automatique, merci de ne pas y repondre.</p>
        </div>
    </div>
</body>
</html>";

        return emailBody;
    }

    /// <summary>
    /// Construit la liste HTML des items de commande
    /// </summary>
    private static string BuildItemsList(List<OrderItemDto> items)
    {
        var itemsHtml = "";
        foreach (var item in items)
        {
            itemsHtml += $@"
                <div class=""item"">
                    <strong>Produit ID:</strong> {item.ProductId}<br>
                    <strong>Quantite:</strong> {item.Quantity}<br>
                    <strong>Prix unitaire:</strong> {item.Price:C2}<br>
                    <strong>Sous-total:</strong> {(item.Price * item.Quantity):C2}
                </div>";
        }
        return itemsHtml;
    }

    /// <summary>
    /// Convertit le statut en libelle francais
    /// </summary>
    private static string GetStatusLabel(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Draft => "Brouillon",
            OrderStatus.Pending => "En attente",
            OrderStatus.Submitted => "Soumise",
            OrderStatus.Cancelled => "Annulee",
            OrderStatus.Confirmed => "Confirmee",
            OrderStatus.Completed => "Terminee",
            OrderStatus.Shipped => "Expediee",
            OrderStatus.Delivered => "Livree",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Convertit le mode de paiement en libelle
    /// </summary>
    private static string GetPaymentMethodLabel(int paymentMethod)
    {
        return paymentMethod switch
        {
            1 => "Carte de credit",
            2 => "PayPal",
            3 => "Virement bancaire",
            _ => "Autre"
        };
    }
}

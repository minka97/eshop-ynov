using Ordering.Domain.Abstractions;
using Ordering.Domain.Models;

namespace Email.API.Events;

/// <summary>
/// Événement d'intégration reçu quand une commande est créée
/// Copie de l'événement depuis Ordering.Domain pour désérialisation
/// </summary>
public record OrderCreatedEvent(Order Order) : IDomainEvent;

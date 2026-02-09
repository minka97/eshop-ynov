# 📧 Email.API - Notifications de Changement de Statut

## ✅ Nouveautés Ajoutées

### 1. Consumer OrderUpdatedEventConsumer

**Fichier** : [`OrderUpdatedEventConsumer.cs`](file:///home/eddin/RiderProjects/eshop-ynov/src/eshop.services/email/Email.API/EventHandlers/OrderUpdatedEventConsumer.cs)

Email.API écoute maintenant **deux événements** RabbitMQ :

| Événement | Quand | Email envoyé |
|-----------|-------|--------------|
| `OrderCreatedEvent` | Création de commande | ✅ Confirmation de commande |
| `OrderUpdatedEvent` | Changement de statut | 📧 Notification de statut |

### 2. Emails Envoyés Selon le Statut

Le consumer `OrderUpdatedEventConsumer` envoie des emails personnalisés selon le statut :

#### ✅ **Confirmed** - Commande Confirmée
- **Sujet** : "✅ Votre commande {OrderName} a été confirmée"
- **Message** : "Votre commande a été confirmée et est en cours de préparation !"
- **Couleur** : Vert (#4CAF50)

#### 🚚 **Shipped** - Commande Expédiée
- **Sujet** : "🚚 Votre commande {OrderName} a été expédiée"
- **Message** : "Votre commande a été expédiée et est en route vers vous !"
- **Couleur** : Bleu (#2196F3)
- **Infos supplémentaires** : Informations de suivi, délai de livraison estimé

#### 📦 **Delivered** - Commande Livrée
- **Sujet** : "📦 Votre commande {OrderName} a été livrée"
- **Message** : "Votre commande a été livrée avec succès !"
- **Couleur** : Vert (#4CAF50)
- **Infos supplémentaires** : Demande d'avis client

#### ❌ **Cancelled** - Commande Annulée
- **Sujet** : "❌ Votre commande {OrderName} a été annulée"
- **Message** : "Votre commande a été annulée"
- **Couleur** : Rouge (#f44336)
- **Infos supplémentaires** : Informations sur le remboursement

#### ✅ **Completed** - Commande Terminée
- **Sujet** : "✅ Votre commande {OrderName} est terminée"
- **Message** : "Votre commande est terminée. Merci pour votre achat !"
- **Couleur** : Vert (#4CAF50)

### 3. Docker Compose Mis à Jour

**Fichier** : [`src/compose.yaml`](file:///home/eddin/RiderProjects/eshop-ynov/src/compose.yaml)

Au lieu de créer un nouveau fichier, j'ai **mis à jour le fichier existant** avec :

✅ **Mailpit** ajouté dans la section "Common Service"
- Port SMTP : 1025
- Port WebUI : 8025
- Network : `email_network`

✅ **Email.API** ajouté dans une nouvelle section "Service Email"
- Ports : 6070, 6071
- Dépendances : RabbitMQ (messageBroker) + Mailpit
- Networks : `email_network` + `messageBroker_network`
- Variables d'environnement SMTP configurées pour Mailpit

✅ **Network email_network** ajouté aux réseaux

## 🚀 Comment Utiliser

### Démarrer tous les services

```bash
# Option 1 : Script automatique
./start-services.sh

# Option 2 : Commande manuelle
cd src
docker compose up -d
```

### Tester les notifications de changement de statut

1. **Créer une commande** via Ordering.API
   - Email de confirmation envoyé automatiquement

2. **Mettre à jour le statut de la commande** (par exemple : `Confirmed` → `Shipped`)
   - Email de notification envoyé automatiquement

3. **Consulter les emails dans Mailpit** : http://localhost:8025

### Exemple de flux complet

```
1. Client crée une commande
   → OrderCreatedEvent publié
   → Email.API envoie : "✅ Confirmation de votre commande"
   
2. Admin confirme la commande (statut = Confirmed)
   → OrderUpdatedEvent publié
   → Email.API envoie : "✅ Votre commande a été confirmée"
   
3. Admin expédie la commande (statut = Shipped)
   → OrderUpdatedEvent publié
   → Email.API envoie : "🚚 Votre commande a été expédiée"
   
4. Commande est livrée (statut = Delivered)
   → OrderUpdatedEvent publié
   → Email.API envoie : "📦 Votre commande a été livrée"
```

## 📧 Consulter les Emails

Ouvrir Mailpit : http://localhost:8025

Vous verrez tous les emails :
- Email de confirmation (OrderCreatedEvent)
- Emails de changement de statut (OrderUpdatedEvent)

## 🏗️ Architecture Finale

```
┌─────────────────┐
│  Ordering.API   │
└────────┬────────┘
         │ Publie OrderCreatedEvent
         │ Publie OrderUpdatedEvent
         ↓
┌─────────────────┐
│   RabbitMQ      │ (messageBroker)
└────────┬────────┘
         │ Events
         ↓
┌─────────────────┐
│   Email.API     │
│  - OrderCreated │
│    EventConsumer│
│  - OrderUpdated │
│    EventConsumer│
└────────┬────────┘
         │ SMTP
         ↓
┌─────────────────┐
│    Mailpit      │
│  (localhost:1025│
│   Web:8025)     │
└─────────────────┘
```

## 📁 Fichiers Créés/Modifiés

### Nouveaux Fichiers
- ✅ `email/Email.API/Events/OrderUpdatedEvent.cs`
- ✅ `email/Email.API/EventHandlers/OrderUpdatedEventConsumer.cs`

### Fichiers Modifiés
- ✅ `src/compose.yaml` - Ajout de Mailpit + Email.API
- ✅ `start-services.sh` - Mise à jour pour utiliser src/compose.yaml
- ❌ `docker-compose.yml` (racine) - Supprimé car redondant

## 🎯 Résumé

Désormais, **Email.API** gère :

1. ✅ **Emails de confirmation** lors de la création de commande
2. ✅ **Emails de notification** lors du changement de statut
3. ✅ **Templates HTML personnalisés** selon le statut
4. ✅ **Déploiement Docker** avec Mailpit pour le développement

Tous les emails sont capturés par **Mailpit** et visibles sur http://localhost:8025 !

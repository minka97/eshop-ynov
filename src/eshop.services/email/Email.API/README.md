# Email.API Microservice

Microservice dédié à l'envoi d'emails pour le projet eShop-YNOV.

## 📋 Description

Email.API est un microservice autonome qui gère l'envoi d'emails de manière asynchrone. Il écoute les événements RabbitMQ (notamment `OrderCreatedEvent`) et envoie des emails de confirmation aux clients.

## 🏗️ Architecture

- **Pattern** : Clean Architecture, CQRS, Event-Driven
- **SMTP** : Mailpit (développement) - Serveur SMTP de test qui capture les emails
- **Message Broker** : RabbitMQ avec MassTransit
- **Framework** : ASP.NET Core 9.0
- **Librairie Email** : MailKit

## 📦 Fonctionnalités

### 1. Consumer RabbitMQ
- Écoute l'événement `OrderCreatedEvent` publié par Ordering.API
- Envoie automatiquement un email de confirmation au client
- Template HTML professionnel avec tous les détails de la commande

### 2. API REST
- **POST** `/api/email/send` - Envoi manuel d'email
- Validation avec FluentValidation
- Documentation via OpenAPI

### 3. Health Checks
- **GET** `/health` - Statut du service

## 🚀 Installation et Démarrage

### Prérequis
- .NET 9.0 SDK
- RabbitMQ (local ou Docker)
- Mailpit (local ou Docker)

### 1. Démarrer Mailpit (Docker)

```bash
docker run -d \
  --name mailpit \
  -p 1025:1025 \
  -p 8025:8025 \
  axllent/mailpit
```

- **SMTP** : `localhost:1025` (pas d'authentification)
- **WebUI** : `http://localhost:8025` (pour voir les emails)

### 2. Démarrer RabbitMQ (Docker)

```bash
docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

- **AMQP** : `localhost:5672`
- **Management UI** : `http://localhost:15672` (guest/guest)

### 3. Démarrer Email.API

```bash
cd /home/eddin/RiderProjects/eshop-ynov/src/eshop.services/email/Email.API
dotnet run
```

Le service démarre sur : `http://localhost:6070`

## 🔧 Configuration

### appsettings.json

```json
{
  "SmtpSettings": {
    "Server": "localhost",
    "Port": 1025,
    "Username": "",
    "Password": "",
    "EnableSsl": false,
    "FromEmail": "noreply@eshop.com",
    "FromName": "eShop YNOV"
  },
  "MessageBroker": {
    "Host": "amqp://localhost:5672",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### Pour SMTP réel (Production)

Remplacer les paramètres par un vrai serveur SMTP :

```json
{
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "noreply@eshop.com",
    "FromName": "eShop YNOV"
  }
}
```

## 📨 Utilisation

### 1. Envoi automatique via RabbitMQ

Créez une commande dans Ordering.API. Email.API recevra l'événement et enverra automatiquement un email.

### 2. Envoi manuel via API

```bash
curl -X POST http://localhost:6070/api/email/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "client@example.com",
    "subject": "Test Email",
    "body": "<h1>Bonjour !</h1><p>Email de test</p>"
  }'
```

### 3. Consulter les emails dans Mailpit

Ouvrez votre navigateur : `http://localhost:8025`

Tous les emails envoyés apparaîtront dans l'interface Mailpit.

## 🐳 Docker

### Build de l'image

```bash
docker build -t email.api:latest \
  -f src/eshop.services/email/Email.API/Dockerfile .
```

### Démarrer avec Docker Compose

Ajoutez au `docker-compose.yml` :

```yaml
email.api:
  image: email.api:latest
  build:
    context: .
    dockerfile: src/eshop.services/email/Email.API/Dockerfile
  ports:
    - "6070:8080"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - SmtpSettings__Server=mailpit
    - SmtpSettings__Port=1025
    - MessageBroker__Host=amqp://rabbitmq:5672
  depends_on:
    - rabbitmq
    - mailpit
  networks:
    - eshop-network

mailpit:
  image: axllent/mailpit
  ports:
    - "1025:1025"
    - "8025:8025"
  networks:
    - eshop-network
```

## 🧪 Tests

### Health Check

```bash
curl http://localhost:6070/health
```

### Test d'envoi d'email

```bash
curl -X POST http://localhost:6070/api/email/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "test@example.com",
    "subject": "Test",
    "body": "<h1>Test</h1>"
  }'
```

### Vérifier dans Mailpit

1. Ouvrir `http://localhost:8025`
2. Voir l'email reçu
3. Inspecter le HTML, headers, etc.

## 📊 Logs

Les logs sont affichés dans la console :

```
📧 Préparation de l'envoi d'email à test@example.com
✅ Email envoyé avec succès à test@example.com
📬 Consultez Mailpit sur http://localhost:8025 pour voir l'email
```

## 🔗 Liens Utiles

- **Email.API Health Check** : http://localhost:6070/health
- **Mailpit WebUI** : http://localhost:8025
- **RabbitMQ Management** : http://localhost:15672

## 🛠️ Dépendances

- **BuildingBlocks** : Librairies partagées (CQRS, Behaviors, Middlewares)
- **BuildingBlocks.Messaging** : Configuration MassTransit/RabbitMQ
- **Ordering.Domain** : Types Order pour désérialiser OrderCreatedEvent
- **MailKit** : Librairie SMTP moderne
- **MassTransit.RabbitMQ** : Consumer RabbitMQ
- **FluentValidation** : Validation des commandes

## 📁 Structure du Projet

```
Email.API/
├── Configuration/
│   └── SmtpSettings.cs          # Configuration SMTP
├── Controllers/
│   └── EmailController.cs       # API REST
├── EventHandlers/
│   └── OrderCreatedEventConsumer.cs  # Consumer RabbitMQ
├── Events/
│   └── OrderCreatedEvent.cs     # Événement d'intégration
├── Features/
│   └── SendEmail/
│       ├── SendEmailCommand.cs  # CQRS Command
│       ├── SendEmailHandler.cs  # MediatR Handler
│       └── SendEmailValidator.cs # FluentValidation
├── Models/
│   └── EmailMessage.cs          # Modèle Email
├── Services/
│   ├── IEmailService.cs         # Interface
│   └── SmtpEmailService.cs      # Implémentation SMTP
├── Program.cs                   # Configuration application
├── appsettings.json             # Configuration
├── Dockerfile                   # Configuration Docker
└── Email.API.csproj             # Définition projet
```

## 🎯 Prochaines Étapes

1. ✅ Implémenter Email.API
2. ⏳ Démarrer Mailpit et RabbitMQ
3. ⏳ Tester l'envoi manuel via API
4. ⏳ Tester l'envoi automatique via OrderCreatedEvent
5. ⏳ Intégrer dans docker-compose.yml
6. ⏳ Vérifier les emails dans Mailpit

## 📝 Notes

- Mailpit est **uniquement pour le développement**. Il ne envoie pas de vrais emails.
- Pour la production, configurez un vrai serveur SMTP (Gmail, SendGrid, etc.)
- Les emails sont stockés en mémoire dans Mailpit et perdus au redémarrage

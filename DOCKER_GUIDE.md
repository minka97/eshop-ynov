# Docker Compose - Guide d'utilisation

## 🚀 Démarrage rapide

### Option 1 : Script automatique (recommandé)

```bash
./start-services.sh
```

### Option 2 : Commandes Docker Compose

```bash
# Démarrer tous les services
docker compose up -d

# Voir les logs
docker compose logs -f

# Arrêter tous les services
docker compose down

# Rebuild et redémarrer
docker compose up -d --build
```

## 📦 Services inclus

| Service | Port(s) | Description |
|---------|---------|-------------|
| **email.api** | 6070, 6071 | Microservice d'envoi d'emails |
| **mailpit** | 1025 (SMTP), 8025 (WebUI) | Serveur SMTP de développement |
| **rabbitmq** | 5672 (AMQP), 15672 (Management) | Message broker |

## 🔗 Accès aux services

- **Email.API** : http://localhost:6070
- **Email.API Health Check** : http://localhost:6070/health
- **Mailpit WebUI** : http://localhost:8025 (📧 voir les emails)
- **RabbitMQ Management** : http://localhost:15672 (guest/guest)

## 🧪 Tester l'envoi d'email

### Via API REST

```bash
curl -X POST http://localhost:6070/api/email/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "test@example.com",
    "subject": "Test",
    "body": "<h1>Hello from Email.API</h1>"
  }'
```

### Via RabbitMQ (automatique)

1. Créer une commande dans Ordering.API
2. L'événement `OrderCreatedEvent` sera publié sur RabbitMQ
3. Email.API recevra l'événement et enverra un email automatiquement
4. Consulter l'email sur http://localhost:8025

## 📧 Consulter les emails dans Mailpit

1. Ouvrir http://localhost:8025
2. Tous les emails envoyés apparaîtront dans la liste
3. Cliquer sur un email pour voir le contenu HTML
4. Inspecter les headers, pièces jointes, etc.

## 🛠️ Commandes utiles

```bash
# Voir les logs d'un service spécifique
docker compose logs -f email.api
docker compose logs -f mailpit
docker compose logs -f rabbitmq

# Redémarrer un service
docker compose restart email.api

# Voir les services actifs
docker compose ps

# Arrêter et supprimer tous les conteneurs, réseaux, volumes
docker compose down -v
```

## 🔧 Configuration

Les variables d'environnement sont définies dans `docker-compose.yml` :

- **SmtpSettings__Server** : `mailpit` (nom du service)
- **SmtpSettings__Port** : `1025`
- **MessageBroker__Host** : `amqp://rabbitmq:5672`

## 📝 Notes

- Mailpit ne envoie **pas** de vrais emails, il les capture pour le développement
- Les emails sont stockés en mémoire et perdus au redémarrage de Mailpit
- RabbitMQ utilise les credentials par défaut (`guest/guest`)

## ⚠️ Dépannage

### Email.API ne démarre pas

```bash
# Vérifier les logs
docker compose logs email.api

# Rebuild l'image
docker compose build email.api
docker compose up -d email.api
```

### RabbitMQ n'est pas prêt

```bash
# Attendre quelques secondes pour le health check
docker compose ps

# Redémarrer Email.API après RabbitMQ
docker compose restart email.api
```

### Port déjà utilisé

Si un port est déjà utilisé, modifier `docker-compose.yml` :

```yaml
ports:
  - "6080:8080"  # Changer 6070 en 6080
```

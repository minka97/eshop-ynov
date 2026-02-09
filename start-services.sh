#!/bin/bash

echo "🚀 Démarrage des services eShop YNOV avec Docker Compose..."
echo ""

# Vérifier si Docker est installé
if ! command -v docker &> /dev/null; then
    echo "❌ Docker n'est pas installé. Veuillez installer Docker d'abord."
    exit 1
fi

# Vérifier si Docker Compose est installé
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo "❌ Docker Compose n'est pas installé. Veuillez installer Docker Compose d'abord."
    exit 1
fi

echo "✅ Docker et Docker Compose sont installés"
echo ""

# Démarrer les services
echo "📦 Lancement des conteneurs..."
cd src
docker compose up -d

echo ""
echo "✅ Services démarrés !"
echo ""
echo "🔗 Accès aux services :"
echo "  - Email.API          : http://localhost:6070"
echo "  - Email.API Health   : http://localhost:6070/health"
echo "  - Mailpit WebUI      : http://localhost:8025"
echo "  - RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo ""
echo "📧 Pour voir les emails envoyés, ouvrez : http://localhost:8025"
echo ""
echo "Pour voir les logs : cd src && docker compose logs -f"
echo "Pour arrêter       : cd src && docker compose down"

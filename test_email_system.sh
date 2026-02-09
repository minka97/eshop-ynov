#!/bin/bash

echo "🧪 Test complet du système Email.API"
echo "====================================="
echo ""

# Vérifier que les services sont démarrés
echo "1️⃣ Vérification des services..."
docker compose ps | grep -E "(mailpit|messageBroker|email.api)" || exit 1

echo "✅ Services démarrés"
echo ""

# Test 1: Envoi manuel d'email
echo "2️⃣ Test d'envoi manuel d'email via API..."
RESPONSE=$(curl -s -X POST http://localhost:6070/api/email/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "test@example.com",
    "subject": "Test Email API",
    "body": "<h1>✅ Email.API fonctionne !</h1><p>Ceci est un email de test.</p>"
  }')

echo "Réponse API: $RESPONSE"

if echo "$RESPONSE" | grep -q "success.*true"; then
  echo "✅ Email envoyé avec succès"
else
  echo "❌ Échec de l'envoi d'email"
  exit 1
fi

echo ""
echo "3️⃣ Consultez Mailpit pour voir l'email : http://localhost:8025"
echo ""

# Instructions pour tester avec Ordering.API
echo "4️⃣ Pour tester avec Ordering.API :"
echo "   a) Démarrer Ordering.API et sa base de données :"
echo "      cd /home/eddin/RiderProjects/eshop-ynov/src"
echo "      docker compose up -d ordering.database ordering.api"
echo ""
echo "   b) Créer une commande (utilisez test_update_order_status.sh ou l'API)"
echo ""
echo "   c) Vérifier les logs Email.API :"
echo "      docker compose logs email.api -f"
echo ""
echo "   d) Mettre à jour le statut de la commande :"
echo "      Utilisez test_update_order_status.sh pour passer la commande de 'Pending' à 'Confirmed', puis 'Shipped', etc."
echo ""
echo "   e) Consultez Mailpit : http://localhost:8025"
echo "      Vous devriez voir :"
echo "      - 1 email de confirmation (OrderCreatedEvent)"
echo "      - N emails de changement de statut (OrderUpdatedEvent)"
echo ""
echo "✅ Test manuel réussi ! Consultez http://localhost:8025 pour voir l'email."

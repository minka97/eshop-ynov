#!/bin/bash

echo "======================================"
echo "Test UpdateOrderStatus Endpoint"
echo "======================================"
echo ""

# Étape 1 : Créer une commande
echo "📦 Étape 1 : Création d'une nouvelle commande..."
ORDER_RESPONSE=$(curl -s -X POST http://localhost:6063/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "58c49479-ec65-4de2-86e7-033c546291aa",
    "orderName": "TEST_ORDER_'$(date +%s)'",
    "shippingAddress": {
      "firstName": "Test",
      "lastName": "User",
      "emailAddress": "test@example.com",
      "addressLine": "123 Test St",
      "country": "France",
      "state": "IDF",
      "zipCode": "75001"
    },
    "billingAddress": {
      "firstName": "Test",
      "lastName": "User",
      "emailAddress": "test@example.com",
      "addressLine": "123 Test St",
      "country": "France",
      "state": "IDF",
      "zipCode": "75001"
    },
    "payment": {
      "cardName": "Test User",
      "cardNumber": "4111111111111111",
      "expiration": "12/25",
      "cvv": "123",
      "paymentMethod": 1
    },
    "orderStatus": 0
  }')

ORDER_ID=$(echo $ORDER_RESPONSE | tr -d '"')
echo "✅ Commande créée avec ID: $ORDER_ID"
echo ""

# Attendre un peu
sleep 1

# Étape 2 : Mettre à jour le statut à "Processing" (1)
echo "🔄 Étape 2 : Mise à jour du statut à 'Processing' (1)..."
RESULT1=$(curl -s -X PATCH http://localhost:6063/orders/${ORDER_ID}/status \
  -H "Content-Type: application/json" \
  -d '{"newStatus": 1}')
echo "   Résultat: $RESULT1"
echo ""

# Attendre un peu
sleep 1

# Étape 3 : Mettre à jour le statut à "Completed" (2)
echo "✅ Étape 3 : Mise à jour du statut à 'Completed' (2)..."
RESULT2=$(curl -s -X PATCH http://localhost:6063/orders/${ORDER_ID}/status \
  -H "Content-Type: application/json" \
  -d '{"newStatus": 2}')
echo "   Résultat: $RESULT2"
echo ""

# Étape 4 : Tester avec un ID inexistant
echo "🧪 Étape 4 : Test avec un ID inexistant..."
FAKE_ID="00000000-0000-0000-0000-000000000000"
RESULT3=$(curl -s -X PATCH http://localhost:6063/orders/${FAKE_ID}/status \
  -H "Content-Type: application/json" \
  -d '{"newStatus": 2}')
echo "   Résultat: $RESULT3"
echo ""

echo "======================================"
echo "✅ Tests terminés !"
echo "======================================"
echo ""
echo "📊 Résumé:"
echo "  - Commande créée: $ORDER_ID"
echo "  - Update 1 (Processing): $RESULT1"
echo "  - Update 2 (Completed): $RESULT2"
echo "  - Test ID invalide: $RESULT3"
echo ""

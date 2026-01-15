# 📄 Feature : Pagination des Produits

> **Branche** : `feat/pagination`  
> **Auteur** : [EDDIN]  
> **Date** : 2026-01-15

## 📋 Description

Implémentation de la pagination pour l'endpoint `GET /products` suivant le pattern **CQRS** avec **MediatR**.

## 🎯 Objectif

Permettre de récupérer les produits par page au lieu de tous les charger d'un coup, améliorant :
- Les performances (moins de données transférées)
- L'expérience utilisateur (chargement plus rapide)
- L'utilisation mémoire (côté serveur et client)

---

## 📁 Fichiers Créés

```
Features/Products/Queries/GetProducts/
├── GetProductsQuery.cs        # Requête avec paramètres
├── GetProductsQueryHandler.cs # Logique de pagination
└── GetProductsQueryResult.cs  # Format de réponse
```

---

## 🔧 Utilisation de l'API

### Endpoint

```http
GET /products?numPage={page}&sizePage={size}
```

### Paramètres

| Paramètre | Type | Défaut | Description |
|-----------|------|--------|-------------|
| `numPage` | int | 1 | Numéro de la page (commence à 1) |
| `sizePage` | int | 12 | Nombre de produits par page |

### Exemple de Requête

```bash
curl "http://localhost:6060/products?numPage=1&sizePage=10"
```

### Exemple de Réponse

```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "iPhone 15",
    "description": "Smartphone Apple",
    "price": 999.99,
    "imageFile": "iphone15.jpg",
    "categories": ["Electronics", "Phones"]
  },
  ...
]
```

---

## 🧩 Architecture CQRS

```
┌─────────────────────────────────────────────────────────────┐
│                      ProductsController                      │
│  sender.Send(new GetProductsQuery(numPage, sizePage))       │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                         MediatR                              │
│            Route vers le Handler correspondant               │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                  GetProductsQueryHandler                     │
│  1. Compte le total de produits                             │
│  2. Skip((numPage-1) * sizePage)                            │
│  3. Take(sizePage)                                          │
│  4. Retourne GetProductsQueryResult                         │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                        PostgreSQL                            │
│                    (via Marten ORM)                          │
└─────────────────────────────────────────────────────────────┘
```

---

## 📝 Code Implémenté

### GetProductsQuery.cs

```csharp
public record GetProductsQuery(int NumPage, int SizePage) 
    : IQuery<GetProductsQueryResult>;
```

### GetProductsQueryHandler.cs

```csharp
public class GetProductsQueryHandler(IDocumentSession documentSession) 
    : IQueryHandler<GetProductsQuery, GetProductsQueryResult>
{
    public async Task<GetProductsQueryResult> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var totalCount = await documentSession
            .Query<Product>()
            .CountAsync(cancellationToken);
        
        var products = await documentSession
            .Query<Product>()
            .Skip((request.NumPage - 1) * request.SizePage)
            .Take(request.SizePage)
            .ToListAsync(cancellationToken);
        
        return new GetProductsQueryResult(
            products, totalCount, request.NumPage, request.SizePage);
    }
}
```

### GetProductsQueryResult.cs

```csharp
public record GetProductsQueryResult(
    IEnumerable<Product> Products,
    int TotalCount,
    int NumPage,
    int SizePage
);
```

---

## 🧮 Formule de Pagination

```
Skip = (NumPage - 1) × SizePage

Exemple avec 25 produits et SizePage = 10:
┌──────────┬──────┬──────┬─────────────────────┐
│ NumPage  │ Skip │ Take │ Produits retournés  │
├──────────┼──────┼──────┼─────────────────────┤
│    1     │  0   │  10  │     1 à 10          │
│    2     │  10  │  10  │    11 à 20          │
│    3     │  20  │  10  │    21 à 25          │
└──────────┴──────┴──────┴─────────────────────┘
```

---

## ✅ Tests

### Avec cURL

```bash
# Page 1
curl "http://localhost:6060/products?numPage=1&sizePage=5"

# Page 2
curl "http://localhost:6060/products?numPage=2&sizePage=5"
```

### Avec Postman

1. Méthode : `GET`
2. URL : `http://localhost:6060/products`
3. Params :
   - `numPage` = `1`
   - `sizePage` = `10`

---

## 🔗 Références

- [Pattern CQRS](../../adr/002-cqrs-mediatr.md)
- [Documentation Marten](https://martendb.io/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)

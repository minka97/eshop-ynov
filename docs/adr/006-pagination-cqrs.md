# ADR-006 : Implémentation de la Pagination avec CQRS

## Statut
**Accepté** - 2026-01-15

## Contexte

L'endpoint `GET /products` du Catalog Service retournait initialement tous les produits de la base de données. Avec l'augmentation du nombre de produits, cette approche pose des problèmes de :
- **Performance** : Temps de réponse élevé
- **Consommation mémoire** : Chargement de tous les produits côté serveur et client
- **Expérience utilisateur** : Temps d'attente trop long

## Décision

Nous avons décidé d'implémenter une **pagination côté serveur** en suivant le pattern CQRS existant.

### Architecture choisie

```
Features/Products/Queries/GetProducts/
├── GetProductsQuery.cs        # Paramètres: NumPage, SizePage
├── GetProductsQueryHandler.cs # Logique: Skip/Take avec Marten
└── GetProductsQueryResult.cs  # Retour: Products, TotalCount
```

### Mécanisme de pagination

```csharp
.Skip((NumPage - 1) * SizePage)
.Take(SizePage)
```

## Alternatives considérées

| Alternative | Avantages | Inconvénients | Décision |
|-------------|-----------|---------------|----------|
| Pagination offset (Skip/Take) | Simple, standard | Performance O(n) sur grands datasets | ✅ Choisi |
| Cursor-based pagination | Performance O(1) | Plus complexe, pas de "page X" | ❌ Rejeté |
| Pagination côté client | Pas de modification API | Charge tout en mémoire | ❌ Rejeté |

## Conséquences

### Positives
- ✅ Temps de réponse réduit (seulement N produits chargés)
- ✅ Cohérence avec le pattern CQRS existant
- ✅ API standard et intuitive (`?numPage=1&sizePage=10`)

### Négatives
- ⚠️ Skip/Take moins performant sur très grands datasets (>100k lignes)
- ⚠️ Nécessite de maintenir `TotalCount` pour le frontend

## Références

- [Documentation Marten - Pagination](https://martendb.io/documents/querying/linq/)
- [ADR-002 : CQRS avec MediatR](./002-cqrs-mediatr.md)

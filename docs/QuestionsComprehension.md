# Questions de Compréhension - Architecture Logicielle

Ce document regroupe les questions posées durant le TP et leurs réponses détaillées.

---

## Table des matières

1. [Pattern CQRS](#1-pattern-cqrs)
2. [Parcours d'une requête GET /products/{id}](#2-parcours-dune-requête-get-productsid)
3. [Les Records et DTOs en C#](#3-les-records-et-dtos-en-c)
4. [Signature d'un Record avec Interface](#4-signature-dun-record-avec-interface)
5. [Marten et PostgreSQL](#5-marten-et-postgresql)
6. [Event Store vs CRUD](#6-event-store-vs-crud)
7. [IDocumentSession et Injection de Dépendances](#7-idocumentsession-et-injection-de-dépendances)

---

## 1. Pattern CQRS

### Question
*"Explique moi le pattern CQRS"*

### Réponse

**CQRS** = **C**ommand **Q**uery **R**esponsibility **S**egregation

C'est un pattern qui **sépare** les opérations de lecture (Query) des opérations d'écriture (Command).

```
┌─────────────────────────────────────────────────────────────────┐
│                      ARCHITECTURE CQRS                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   QUERY (Lecture)              COMMAND (Écriture)               │
│   ─────────────────            ──────────────────               │
│   • GetProductById             • CreateProduct                  │
│   • GetProducts                • UpdateProduct                  │
│   • GetProductsByCategory      • DeleteProduct                  │
│                                                                  │
│   → Ne modifie RIEN            → Modifie les données            │
│   → Retourne des données       → Retourne succès/échec          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Pourquoi utiliser CQRS ?

| Avantage | Description |
|----------|-------------|
| Séparation des responsabilités | Chaque classe a UN seul rôle |
| Testabilité | Facile de tester chaque handler isolément |
| Scalabilité | On peut optimiser lectures et écritures séparément |
| Maintenabilité | Code organisé par fonctionnalité |

---

## 2. Parcours d'une requête GET /products/{id}

### Question
*"Pour un parcours GET ../product/id, quel est le parcours ?"*

### Réponse

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                    PARCOURS D'UNE REQUÊTE GET /products/{id}                 │
└──────────────────────────────────────────────────────────────────────────────┘

    Client HTTP
         │
         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ 1. ROUTAGE ASP.NET CORE                                                     │
│    GET /products/550e8400-e29b-41d4-a716-446655440000                       │
│    Le framework analyse l'URL et trouve [HttpGet("{id:guid}")]              │
└─────────────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ 2. CONTRÔLEUR (ProductsController.cs)                                       │
│    public async Task<ActionResult<Product>> GetProductById(Guid id)         │
│    {                                                                        │
│        var result = await sender.Send(new GetProductByIdQuery(id));         │
│        return Ok(result.Product);                                           │
│    }                                                                        │
└─────────────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ 3. QUERY (GetProductByIdQuery.cs)                                           │
│    public record GetProductByIdQuery(Guid Id)                               │
│        : IQuery<GetProductByIdQueryResult>;                                 │
│    → Simple objet "record" qui transporte l'ID                              │
└─────────────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ 4. MEDIATR - Le "routeur" interne                                           │
│    1. Reçoit GetProductByIdQuery                                            │
│    2. Cherche le handler correspondant                                      │
│    3. Exécute les Behaviors (Validation → Logging → Handler)                │
└─────────────────────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ 5. QUERY HANDLER (GetProductByIdQueryHandler.cs)                            │
│    var product = await session.LoadAsync<Product>(request.Id);              │
│    if (product is null) throw new ProductNotFoundException(request.Id);     │
│    return new GetProductByIdQueryResult(product);                           │
└─────────────────────────────────────────────────────────────────────────────┘
         │
         ▼
    Client HTTP reçoit: { "id": "...", "name": "iPhone", "price": 999 }
```

### Fichiers impliqués

| Étape | Fichier | Rôle |
|-------|---------|------|
| 1 | Program.cs | Configure MediatR et le routage |
| 2 | ProductsController.cs | Point d'entrée HTTP |
| 3 | GetProductByIdQuery.cs | Objet "message" de la requête |
| 4 | GetProductByIdQueryHandler.cs | Logique métier (accès DB) |

---

## 3. Les Records et DTOs en C#

### Question
*"Je ne comprends pas le `public record GetProductByIdQuery(Guid Id)`, il n'y a pas d'implémentation, du coup lorsqu'il est appelé il fait rien"*

### Réponse

Le `record` n'a **PAS besoin** d'implémentation car c'est **juste un conteneur de données** (un "message"). Il ne fait **rien** par lui-même, et c'est **voulu**.

```csharp
// CE QUE VOUS VOYEZ (syntaxe moderne C# 9+)
public record GetProductByIdQuery(Guid Id);

// CE QUE LE COMPILATEUR GÉNÈRE (équivalent complet)
public class GetProductByIdQuery
{
    public Guid Id { get; init; }  // ← Le champ EST LÀ !

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }

    // + Equals, GetHashCode, ToString auto-générés
}
```

### Qui fait le travail alors ?

C'est **MediatR** qui fait la magie :

```
1. Vous appelez : sender.Send(new GetProductByIdQuery(id))

2. MediatR reçoit cet objet et cherche un Handler

3. MediatR trouve : GetProductByIdQueryHandler

4. MediatR appelle : handler.Handle(query, cancellationToken)

5. Le Handler fait le vrai travail (accès DB, etc.)
```

### Analogie simple - Système postal

| Élément | Rôle |
|---------|------|
| `GetProductByIdQuery` | **L'enveloppe** - contient l'ID, ne fait rien |
| `MediatR` | **La Poste** - route l'enveloppe vers le bon destinataire |
| `GetProductByIdQueryHandler` | **Le destinataire** - ouvre l'enveloppe et fait le travail |

---

## 4. Signature d'un Record avec Interface

### Question
*"Peut tu m'expliquer la signature `public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdQueryResult>;`"*

### Réponse

```csharp
public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdQueryResult>;
│      │      │                   │         │ │      │
│      │      │                   │         │ │      └── Type de retour attendu
│      │      │                   │         │ └── Interface implémentée
│      │      │                   │         └── Héritage/Implémentation
│      │      │                   └── Paramètre (devient propriété)
│      │      └── Nom de la classe
│      └── Type spécial (immutable, avec Equals/GetHashCode auto)
└── Modificateur d'accès
```

### Explication de chaque partie

| Partie | Signification |
|--------|---------------|
| `public` | Accessible partout dans le code |
| `record` | Type spécial C# 9+ (classe immutable avec bonus) |
| `GetProductByIdQuery` | Nom de la classe |
| `(Guid Id)` | Constructeur + propriété `Id` auto-générés |
| `:` | "implémente" ou "hérite de" |
| `IQuery<GetProductByIdQueryResult>` | Interface qui dit "je suis une Query qui retourne ce type" |

### Le générique `<GetProductByIdQueryResult>`

C'est un **contrat** qui dit à MediatR :
- **Input** : `GetProductByIdQuery` (contient un `Guid Id`)
- **Output** : `GetProductByIdQueryResult` (contiendra un `Product`)

---

## 5. Marten et PostgreSQL

### Question
*"Peut tu m'expliquer le Marten dans le fichier Program.cs, qu'est ce qu'il fait pour une requête ?"*

### Réponse

**Marten** est une librairie .NET qui transforme **PostgreSQL** en :
- **Document Database** (comme MongoDB)
- **Event Store** (pour Event Sourcing)

### Configuration dans Program.cs

```csharp
builder.Services.AddMarten(options =>
    {
        options.Connection(configuration.GetConnectionString("CatalogConnection") ?? string.Empty);
    })
    .UseLightweightSessions();
```

| Partie | Rôle |
|--------|------|
| `AddMarten(options => ...)` | Enregistre Marten dans l'injection de dépendances |
| `options.Connection(...)` | Configure la connexion à PostgreSQL |
| `.UseLightweightSessions()` | Utilise des sessions légères (plus performantes) |

### Comment Marten stocke les données

```
Votre code C#                           PostgreSQL
─────────────                           ──────────

Product product = new Product           Table: mt_doc_product
{                                       ┌─────────────────────────────────┐
    Id = Guid.NewGuid(),        ──►     │ id (UUID) │ data (JSONB)        │
    Name = "iPhone",                    ├───────────┼─────────────────────┤
    Price = 999                         │ abc-123   │ {"Name":"iPhone",   │
};                                      │           │  "Price":999, ...}  │
                                        └─────────────────────────────────┘
```

### Opérations Marten

| Opération | Méthode | SQL généré |
|-----------|---------|------------|
| Charger par ID | `session.LoadAsync<Product>(id)` | `SELECT data FROM mt_doc_product WHERE id = 'xxx'` |
| Requête LINQ | `session.Query<Product>().Where(...)` | `SELECT data FROM mt_doc_product WHERE data->>'Price' > 100` |
| Créer | `session.Store(product)` | `INSERT INTO mt_doc_product (id, data) VALUES (...)` |
| Mettre à jour | `session.Update(product)` | `UPDATE mt_doc_product SET data = '{...}' WHERE id = 'xxx'` |
| Supprimer | `session.Delete<Product>(id)` | `DELETE FROM mt_doc_product WHERE id = 'xxx'` |
| Exécuter | `session.SaveChangesAsync()` | Exécute toutes les opérations en attente |

---

## 6. Event Store vs CRUD

### Question
*"C'est quoi le Event Store ?"*

### Réponse

Un **Event Store** est une base de données qui stocke l'**historique complet** de tous les changements (événements) d'une entité, au lieu de stocker uniquement son état actuel.

### Comparaison

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    APPROCHE CRUD CLASSIQUE                                  │
│                    (Ce que vous utilisez actuellement)                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   Table: Products                                                           │
│   ┌──────────┬─────────────────┬─────────┐                                  │
│   │ Id       │ Name            │ Price   │                                  │
│   ├──────────┼─────────────────┼─────────┤                                  │
│   │ abc-123  │ iPhone 15 Pro   │ 1199    │  ◄── État ACTUEL uniquement      │
│   └──────────┴─────────────────┴─────────┘                                  │
│                                                                             │
│   Problème: On ne sait pas que le prix était 999€ avant !                   │
│   L'historique est PERDU.                                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                    APPROCHE EVENT SOURCING                                  │
│                    (Avec Event Store)                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   Table: Events (Event Store)                                               │
│   ┌──────────┬─────────────────────────┬──────────────────────────────────┐ │
│   │ Id       │ EventType               │ Data                             │ │
│   ├──────────┼─────────────────────────┼──────────────────────────────────┤ │
│   │ abc-123  │ ProductCreated          │ {Name:"iPhone", Price:999}       │ │
│   │ abc-123  │ ProductRenamed          │ {NewName:"iPhone 15"}            │ │
│   │ abc-123  │ ProductPriceChanged     │ {OldPrice:999, NewPrice:1099}    │ │
│   │ abc-123  │ ProductRenamed          │ {NewName:"iPhone 15 Pro"}        │ │
│   │ abc-123  │ ProductPriceChanged     │ {OldPrice:1099, NewPrice:1199}   │ │
│   └──────────┴─────────────────────────┴──────────────────────────────────┘ │
│                                                                             │
│   Avantage: On a TOUT l'historique !                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Analogie simple

| Approche | Analogie |
|----------|----------|
| **CRUD** | Photo instantanée (on voit l'état actuel) |
| **Event Sourcing** | Film complet (on voit tout ce qui s'est passé) |

### Cas d'usage de l'Event Sourcing

| Cas d'usage | Pourquoi Event Sourcing ? |
|-------------|---------------------------|
| **Banque** | Historique complet des transactions |
| **E-commerce** | Suivi des commandes (créée → payée → expédiée) |
| **Audit** | Qui a fait quoi et quand ? |
| **Replay** | Rejouer les événements pour corriger des bugs |

---

## 7. IDocumentSession et Injection de Dépendances

### Question
*"IDocumentSession documentSession, c'est quoi documentSession, est-ce un objet Marten et aussi comment ils font l'injection de dépendances ?"*

### Réponse

Oui, `IDocumentSession` est une **interface de Marten**. C'est l'objet principal pour interagir avec la base de données.

### Méthodes disponibles

| Méthode | Description |
|---------|-------------|
| `LoadAsync<T>(id)` | Charger un document par ID |
| `Query<T>()` | Requêtes LINQ |
| `Store(entity)` | Insérer un nouveau document |
| `Update(entity)` | Mettre à jour un document |
| `Delete<T>(id)` | Supprimer un document |
| `SaveChangesAsync()` | Exécuter toutes les opérations |

### Comment fonctionne l'injection de dépendances

#### Étape 1 : Enregistrement dans Program.cs

```csharp
builder.Services.AddMarten(options =>
    {
        options.Connection(configuration.GetConnectionString("CatalogConnection") ?? string.Empty);
    })
    .UseLightweightSessions();
```

`AddMarten()` enregistre automatiquement :
- `IDocumentStore` (singleton)
- `IDocumentSession` (scoped - une instance par requête HTTP)

#### Étape 2 : Injection dans le Handler

```csharp
// Syntaxe "Primary Constructor" (C# 12)
public class UpdateProductCommandHandler(IDocumentSession documentSession)

// Équivalent en syntaxe classique
public class UpdateProductCommandHandler
{
    private readonly IDocumentSession _documentSession;

    public UpdateProductCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }
}
```

#### Étape 3 : Résolution automatique

```
Requête HTTP arrive
       │
       ▼
ASP.NET crée un "Scope" pour cette requête
       │
       ▼
MediatR doit créer UpdateProductCommandHandler
       │
       ▼
Le conteneur DI voit : "Il me faut IDocumentSession"
       │
       ▼
Marten a enregistré IDocumentSession → crée une instance
       │
       ▼
Le Handler reçoit l'instance de IDocumentSession
       │
       ▼
Fin de la requête → le Scope est détruit → Session fermée
```

### Cycle de vie des services

| Service | Cycle de vie | Description |
|---------|--------------|-------------|
| `IDocumentStore` | **Singleton** | Une seule instance pour toute l'application |
| `IDocumentSession` | **Scoped** | Une instance par requête HTTP |

---

## Résumé des concepts clés

| Concept | Description |
|---------|-------------|
| **CQRS** | Sépare lectures (Query) et écritures (Command) |
| **MediatR** | Routeur qui connecte Commands/Queries aux Handlers |
| **Record** | Classe immutable auto-générée (C# 9+) |
| **DTO** | Data Transfer Object - transporte des données sans logique |
| **Marten** | ORM Document pour PostgreSQL |
| **Event Store** | Base de données qui stocke les événements (historique) |
| **IDocumentSession** | Interface Marten pour les opérations CRUD |
| **Injection de dépendances** | Le framework fournit automatiquement les objets nécessaires |
| **Primary Constructor** | Syntaxe C# 12 pour simplifier l'injection |

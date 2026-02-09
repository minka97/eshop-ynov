# Architecture - Communication Catalog ↔ Discount via gRPC

## Diagramme de Composants

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                        CLIENT                                            │
│                                  (Browser / Postman)                                     │
└────────────────────────────────────┬────────────────────────────────────────────────────┘
                                     │
                                     │ HTTP/REST
                                     │ GET /products/{id}
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              CATALOG.API SERVICE                                         │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐ │
│  │                           API Layer (ASP.NET Core)                                 │ │
│  │  ┌─────────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                       ProductsController                                     │  │ │
│  │  │  - GetProductById(Guid id)                                                   │  │ │
│  │  └───────────────────────────────┬─────────────────────────────────────────────┘  │ │
│  └──────────────────────────────────┼────────────────────────────────────────────────┘ │
│                                     │                                                   │
│                                     │ MediatR (CQRS)                                    │
│                                     │ Send(GetProductByIdQuery)                         │
│                                     ▼                                                   │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                          Application Layer (CQRS)                                │  │
│  │  ┌─────────────────────────────────────────────────────────────────────────────┐ │  │
│  │  │                    GetProductByIdQueryHandler                                │ │  │
│  │  │                                                                              │ │  │
│  │  │  1. LoadAsync<Product>(id)              ──────┐                             │ │  │
│  │  │                                                │                             │ │  │
│  │  │  2. GetDiscountAsync(productName)      ───────┼──────┐                      │ │  │
│  │  │                                                │      │                      │ │  │
│  │  │  3. Enrich Product with Discount               │      │                      │ │  │
│  │  │     - DiscountDescription                      │      │                      │ │  │
│  │  │     - DiscountAmount                           │      │                      │ │  │
│  │  │     - FinalPrice (calculated)                  │      │                      │ │  │
│  │  │                                                │      │                      │ │  │
│  │  │  4. Return enriched Product                    │      │                      │ │  │
│  │  └────────────────────────────────────────────────┼──────┼──────────────────────┘ │  │
│  └───────────────────────────────────────────────────┼──────┼────────────────────────┘  │
│                                                       │      │                           │
│                                                       │      │                           │
│  ┌────────────────────────────────────────────────┐  │      │                           │
│  │         Domain Layer (Models)                  │  │      │                           │
│  │  ┌──────────────────────────────────────────┐  │  │      │                           │
│  │  │            Product                       │  │  │      │                           │
│  │  │  - Id: Guid                              │◄─┘      │                           │
│  │  │  - Name: string                          │         │                           │
│  │  │  - Description: string                   │         │                           │
│  │  │  - Price: decimal                        │         │                           │
│  │  │  - Categories: List<string>              │         │                           │
│  │  │  - ImageFile: string                     │         │                           │
│  │  │  - DiscountDescription: string?          │         │                           │
│  │  │  - DiscountAmount: decimal               │         │                           │
│  │  │  - FinalPrice: decimal (computed)        │         │                           │
│  │  └──────────────────────────────────────────┘         │                           │
│  └────────────────────────────────────────────────┘       │                           │
│                                                            │                           │
│  ┌────────────────────────────────────────────────────────┼───────────────────────┐   │
│  │              Infrastructure Layer (Services)           │                       │   │
│  │  ┌──────────────────────────────────────────────────┐  │                       │   │
│  │  │          IDiscountService (Interface)            │  │                       │   │
│  │  │  + GetDiscountAsync(productName): CouponModel?   │  │                       │   │
│  │  └────────────────────┬─────────────────────────────┘  │                       │   │
│  │                       │                                 │                       │   │
│  │                       │ implements                      │                       │   │
│  │                       ▼                                 │                       │   │
│  │  ┌──────────────────────────────────────────────────┐  │                       │   │
│  │  │              DiscountService                     │  │                       │   │
│  │  │                                                  │  │                       │   │
│  │  │  - _client: DiscountProtoServiceClient          │◄─┘                       │   │
│  │  │  - _logger: ILogger                              │                          │   │
│  │  │                                                  │                          │   │
│  │  │  + GetDiscountAsync(productName)                │                          │   │
│  │  │    try:                                          │                          │   │
│  │  │      1. Create GetDiscountRequest               │                          │   │
│  │  │      2. Call gRPC: _client.GetDiscountAsync()   │──────────┐               │   │
│  │  │      3. Return CouponModel                       │          │               │   │
│  │  │    catch:                                        │          │               │   │
│  │  │      - Log warning                               │          │               │   │
│  │  │      - Return null (resilience)                  │          │               │   │
│  │  └──────────────────────────────────────────────────┘          │               │   │
│  └─────────────────────────────────────────────────────────────────┼───────────────┘   │
│                                                                     │                   │
│  ┌──────────────────────────────────────────────────────────────┐  │                   │
│  │           Data Access Layer (Marten/PostgreSQL)              │  │                   │
│  │  ┌────────────────────────────────────────────────────────┐  │  │                   │
│  │  │         IDocumentSession (Marten)                      │  │  │                   │
│  │  │  + LoadAsync<Product>(id)                              │  │  │                   │
│  │  └────────────────────────────────────────────────────────┘  │  │                   │
│  └─────────────────────────────────┬────────────────────────────┘  │                   │
└────────────────────────────────────┼───────────────────────────────┼───────────────────┘
                                     │                               │
                                     │                               │ gRPC/HTTP2
                                     ▼                               │ Protocol Buffers
┌────────────────────────────────────────────────────────┐           │
│           PostgreSQL Database                          │           │
│            (CatalogDb)                                 │           │
│  ┌──────────────────────────────────────────────────┐  │           │
│  │  Table: mt_doc_product                           │  │           │
│  │  - id (UUID)                                     │  │           │
│  │  - data (JSONB) → Product JSON                   │  │           │
│  │  - mt_last_modified                              │  │           │
│  │  - mt_version                                    │  │           │
│  └──────────────────────────────────────────────────┘  │           │
└────────────────────────────────────────────────────────┘           │
                                                                     │
                                                                     │
             ┌───────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                             DISCOUNT.GRPC SERVICE                                        │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐ │
│  │                        gRPC Layer (Grpc.AspNetCore)                               │ │
│  │  ┌─────────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                   DiscountProtoService (gRPC Server)                        │  │ │
│  │  │                                                                             │  │ │
│  │  │  Service Definition (discount.proto):                                      │  │ │
│  │  │    rpc GetDiscount(GetDiscountRequest) returns (CouponModel)               │  │ │
│  │  │    rpc CreateDiscount(CreateDiscountRequest) returns (CouponModel)         │  │ │
│  │  │    rpc UpdateDiscount(UpdateDiscountRequest) returns (CouponModel)         │  │ │
│  │  │    rpc DeleteDiscount(DeleteDiscountRequest) returns (DeleteResponse)      │  │ │
│  │  │                                                                             │  │ │
│  │  │  Implementation:                                                            │  │ │
│  │  │    + GetDiscount(GetDiscountRequest request)                               │  │ │
│  │  │      1. Extract ProductName from request                                   │  │ │
│  │  │      2. Query SQLite database                                              │  │ │
│  │  │      3. Return CouponModel (or default if not found)                       │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                         │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐ │
│  │                         Protocol Buffers Messages                                 │ │
│  │  ┌─────────────────────────────────────────────────────────────────────────────┐  │ │
│  │  │  message GetDiscountRequest {                                               │  │ │
│  │  │    string productName = 1;                                                  │  │ │
│  │  │  }                                                                           │  │ │
│  │  │                                                                              │  │ │
│  │  │  message CouponModel {                                                      │  │ │
│  │  │    int32 id = 1;                                                            │  │ │
│  │  │    string productName = 2;                                                  │  │ │
│  │  │    string description = 3;                                                  │  │ │
│  │  │    double amount = 4;                                                       │  │ │
│  │  │  }                                                                           │  │ │
│  │  └─────────────────────────────────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                         │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐ │
│  │                      Data Access Layer (SQLite)                                   │ │
│  │  - Query Coupons by ProductName                                                   │ │
│  │  - CRUD operations on Coupons table                                               │ │
│  └──────────────────────────────────────┬────────────────────────────────────────────┘ │
└─────────────────────────────────────────┼──────────────────────────────────────────────┘
                                          │
                                          ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                           SQLite Database (discountDatabase)                            │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐ │
│  │  Table: Coupons                                                                   │ │
│  │    - Id (INTEGER PRIMARY KEY)                                                     │ │
│  │    - ProductName (TEXT) ← Clé de recherche                                        │ │
│  │    - Description (TEXT)                                                           │ │
│  │    - Amount (REAL)                                                                │ │
│  └───────────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

## Flux de Communication (Séquence)

```
Client          Catalog.API          QueryHandler      DiscountService      Discount.gRPC       SQLite
  │                 │                      │                  │                   │              │
  │─────GET────────>│                      │                  │                   │              │
  │ /products/{id}  │                      │                  │                   │              │
  │                 │                      │                  │                   │              │
  │                 │────MediatR Send─────>│                  │                   │              │
  │                 │  GetProductByIdQuery │                  │                   │              │
  │                 │                      │                  │                   │              │
  │                 │                      │─────LoadAsync───>│                   │              │
  │                 │                      │  (Marten)        │                   │              │
  │                 │                      │                  │                   │              │
  │                 │                      │<────Product──────┤                   │              │
  │                 │                      │                  │                   │              │
  │                 │                      │                  │                   │              │
  │                 │                      │──GetDiscountAsync──────>│            │              │
  │                 │                      │   (product.Name)        │            │              │
  │                 │                      │                         │            │              │
  │                 │                      │                         │──gRPC─────>│              │
  │                 │                      │                         │ GetDiscount│              │
  │                 │                      │                         │            │              │
  │                 │                      │                         │            │──Query──────>│
  │                 │                      │                         │            │ ProductName  │
  │                 │                      │                         │            │              │
  │                 │                      │                         │            │<─CouponData──┤
  │                 │                      │                         │            │              │
  │                 │                      │                         │<─CouponModel─────────────┤
  │                 │                      │                         │            │              │
  │                 │                      │<────CouponModel?────────┤            │              │
  │                 │                      │                         │            │              │
  │                 │                      │                         │            │              │
  │                 │                      │  Enrich Product:        │            │              │
  │                 │                      │  - DiscountDescription  │            │              │
  │                 │                      │  - DiscountAmount       │            │              │
  │                 │                      │  - FinalPrice           │            │              │
  │                 │                      │                         │            │              │
  │                 │<─EnrichedProduct─────┤                         │            │              │
  │                 │                      │                         │            │              │
  │<─200 OK─────────┤                      │                         │            │              │
  │ {Product+Discount}                     │                         │            │              │
  │                 │                      │                         │            │              │
```

## Technologies Utilisées

### Catalog.API
- **Framework**: ASP.NET Core 9.0
- **Pattern**: CQRS (MediatR)
- **ORM**: Marten (PostgreSQL as Document DB)
- **gRPC Client**: Grpc.Net.Client + Google.Protobuf
- **Validation**: FluentValidation
- **Logging**: ILogger (ASP.NET Core)

### Discount.Grpc
- **Framework**: ASP.NET Core 9.0 (gRPC Server)
- **Protocol**: gRPC/HTTP2 + Protocol Buffers
- **Database**: SQLite (file-based)
- **Schema**: discount.proto

## Avantages de cette Architecture

1. **Performance**: gRPC utilise HTTP/2 et Protocol Buffers (binaire) → plus rapide que REST/JSON
2. **Typage fort**: Le contrat est défini dans .proto → génération de code typé
3. **Résilience**: Si Discount est down, Catalog retourne le produit sans discount (graceful degradation)
4. **Découplage**: Les services communiquent via un contrat d'interface clair
5. **Scalabilité**: Chaque service peut être scalé indépendamment

## Configuration

### Program.cs (Catalog.API)
```csharp
// Configuration du client gRPC
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri("http://discount.grpc:6062");
});

builder.Services.AddScoped<IDiscountService, DiscountService>();
```

### Docker Compose
```yaml
catalog.api:
  environment:
    - GrpcSettings__DiscountUrl=http://discount.grpc:6062
```

---

**Généré le**: 2026-02-03
**Architecture**: Microservices avec communication gRPC

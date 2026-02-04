# Architecture Catalog ↔ Discount - Diagrammes Mermaid

## Diagramme de Composants (C4 Model)

```mermaid
graph TB
    subgraph "Client Layer"
        Client[Browser/Postman]
    end

    subgraph "Catalog.API Service"
        subgraph "API Layer"
            Controller[ProductsController<br/>GetProductById]
        end

        subgraph "Application Layer - CQRS"
            QueryHandler[GetProductByIdQueryHandler<br/>1. LoadAsync Product<br/>2. GetDiscountAsync<br/>3. Enrich Product<br/>4. Return Result]
        end

        subgraph "Domain Layer"
            Product[Product Model<br/>+ Id, Name, Price<br/>+ DiscountDescription<br/>+ DiscountAmount<br/>+ FinalPrice]
        end

        subgraph "Infrastructure Layer"
            IDiscountService[IDiscountService Interface]
            DiscountService[DiscountService<br/>- DiscountProtoServiceClient<br/>+ GetDiscountAsync]
        end

        subgraph "Data Access"
            Marten[IDocumentSession<br/>Marten ORM]
        end
    end

    subgraph "PostgreSQL"
        CatalogDB[(CatalogDb<br/>mt_doc_product)]
    end

    subgraph "Discount.Grpc Service"
        subgraph "gRPC Layer"
            GrpcServer[DiscountProtoService<br/>gRPC Server<br/>+ GetDiscount<br/>+ CreateDiscount<br/>+ UpdateDiscount<br/>+ DeleteDiscount]
        end

        subgraph "Protocol Buffers"
            ProtoMessages[GetDiscountRequest<br/>CouponModel<br/>ProductName, Description, Amount]
        end

        subgraph "Data Access Layer"
            DiscountData[SQLite Data Access<br/>Query by ProductName]
        end
    end

    subgraph "SQLite"
        DiscountDB[(discountDatabase<br/>Coupons Table)]
    end

    %% Connections
    Client -->|HTTP GET /products/{id}| Controller
    Controller -->|MediatR Send| QueryHandler
    QueryHandler -->|LoadAsync| Marten
    Marten -->|Query| CatalogDB
    QueryHandler -->|GetDiscountAsync| IDiscountService
    IDiscountService -.->|implements| DiscountService
    DiscountService -->|gRPC Call<br/>HTTP/2 + Protobuf| GrpcServer
    GrpcServer -->|Use| ProtoMessages
    GrpcServer -->|Query| DiscountData
    DiscountData -->|SELECT| DiscountDB
    QueryHandler -->|Uses| Product

    %% Styling
    classDef apiLayer fill:#e1f5ff,stroke:#01579b,stroke-width:2px
    classDef domainLayer fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef infraLayer fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef dataLayer fill:#e8f5e9,stroke:#1b5e20,stroke-width:2px
    classDef grpcLayer fill:#fce4ec,stroke:#880e4f,stroke-width:2px

    class Controller apiLayer
    class QueryHandler,Product domainLayer
    class IDiscountService,DiscountService,Marten infraLayer
    class CatalogDB,DiscountDB dataLayer
    class GrpcServer,ProtoMessages grpcLayer
```

## Diagramme de Séquence

```mermaid
sequenceDiagram
    participant Client as Client<br/>(Browser/Postman)
    participant Controller as ProductsController
    participant MediatR as MediatR
    participant Handler as GetProductByIdQueryHandler
    participant Marten as IDocumentSession<br/>(Marten)
    participant CatalogDB as PostgreSQL<br/>CatalogDb
    participant DiscountSvc as DiscountService
    participant GrpcClient as gRPC Client
    participant GrpcServer as Discount.Grpc<br/>Service
    participant SQLite as SQLite DB<br/>discountDatabase

    %% Main flow
    Client->>+Controller: GET /products/{id}
    Controller->>+MediatR: Send(GetProductByIdQuery)
    MediatR->>+Handler: Handle(query)

    %% Load Product
    Handler->>+Marten: LoadAsync<Product>(id)
    Marten->>+CatalogDB: SELECT * FROM mt_doc_product<br/>WHERE id = {id}
    CatalogDB-->>-Marten: Product JSON
    Marten-->>-Handler: Product entity

    %% Get Discount
    Handler->>+DiscountSvc: GetDiscountAsync(productName)
    Note over DiscountSvc: Try to get discount<br/>with resilience

    DiscountSvc->>+GrpcClient: GetDiscountAsync(request)
    GrpcClient->>+GrpcServer: gRPC Call (HTTP/2)<br/>GetDiscount(GetDiscountRequest)

    GrpcServer->>+SQLite: SELECT * FROM Coupons<br/>WHERE ProductName = ?
    SQLite-->>-GrpcServer: Coupon data or null

    GrpcServer-->>-GrpcClient: CouponModel (Protobuf)
    GrpcClient-->>-DiscountSvc: CouponModel?

    alt Discount found
        DiscountSvc-->>Handler: CouponModel
        Note over Handler: Enrich Product:<br/>- DiscountDescription<br/>- DiscountAmount<br/>- FinalPrice (computed)
    else Discount not found or error
        DiscountSvc-->>Handler: null
        Note over Handler: Product returned<br/>without discount info
    end

    Handler-->>-MediatR: GetProductByIdQueryResult
    MediatR-->>-Controller: Result
    Controller-->>-Client: 200 OK<br/>Product JSON (with discount)

    %% Styling
    rect rgb(230, 245, 255)
        Note over Client,Controller: API Layer
    end

    rect rgb(255, 243, 230)
        Note over Handler,DiscountSvc: Application + Infrastructure
    end

    rect rgb(252, 228, 236)
        Note over GrpcClient,GrpcServer: gRPC Communication (HTTP/2)
    end
```

## Diagramme d'Architecture Simplifiée

```mermaid
flowchart LR
    subgraph Client
        A[Client HTTP]
    end

    subgraph CatalogAPI["Catalog.API (Port 6060)"]
        B[Controller]
        C[CQRS Handler]
        D[DiscountService]
        E[(PostgreSQL<br/>CatalogDb)]
    end

    subgraph DiscountGrpc["Discount.Grpc (Port 6062)"]
        F[gRPC Server]
        G[(SQLite<br/>discountDatabase)]
    end

    A -->|1. HTTP/REST<br/>GET /products/{id}| B
    B -->|2. MediatR| C
    C -->|3. Load Product| E
    C -->|4. Get Discount| D
    D -.->|5. gRPC/HTTP2<br/>Protocol Buffers| F
    F -->|6. Query| G
    G -.->|7. Coupon| F
    F -.->|8. CouponModel| D
    D -->|9. Discount Info| C
    C -->|10. Enriched Product| B
    B -->|11. JSON Response| A

    style A fill:#e3f2fd
    style CatalogAPI fill:#fff3e0
    style DiscountGrpc fill:#fce4ec
    style E fill:#c8e6c9
    style G fill:#c8e6c9
```

## Diagramme de Déploiement Docker

```mermaid
graph TB
    subgraph "Docker Network: catalog_network"
        CatalogAPI[catalog.api<br/>Container<br/>Port: 6060]
        CatalogDB[(catalog.database<br/>PostgreSQL<br/>Port: 5432)]
    end

    subgraph "Docker Network: discount_network"
        DiscountAPI[discount.grpc<br/>Container<br/>Port: 6062]
        DiscountDB[(SQLite<br/>File: /app/discountDatabase)]
    end

    subgraph "Docker Network: basket_network"
        BasketAPI[basket.api<br/>Container<br/>Port: 6061]
        BasketDB[(basket.database<br/>PostgreSQL<br/>Port: 5433)]
        Redis[(basket.redis<br/>Port: 6379)]
    end

    Host[Host Machine<br/>localhost]

    CatalogAPI -->|gRPC<br/>http://discount.grpc:6062| DiscountAPI
    CatalogAPI --> CatalogDB
    DiscountAPI --> DiscountDB
    BasketAPI --> BasketDB
    BasketAPI --> Redis

    Host -->|6060| CatalogAPI
    Host -->|6061| BasketAPI
    Host -->|6062| DiscountAPI

    style CatalogAPI fill:#e1f5ff
    style DiscountAPI fill:#fce4ec
    style BasketAPI fill:#f3e5f5
    style CatalogDB fill:#c8e6c9
    style BasketDB fill:#c8e6c9
    style Redis fill:#ffccbc
```

## Vue d'Ensemble - Pattern CQRS

```mermaid
graph LR
    subgraph "CQRS Pattern in Catalog.API"
        Command[Commands<br/>CreateProduct<br/>UpdateProduct<br/>DeleteProduct]
        Query[Queries<br/>GetProductById<br/>GetProducts<br/>GetByCategory]

        CommandHandler[Command Handlers<br/>Write Operations]
        QueryHandler[Query Handlers<br/>Read Operations<br/>+ Discount Enrichment]

        WriteDB[(Write Database<br/>PostgreSQL)]
        ReadDB[(Read Database<br/>PostgreSQL<br/>Same as Write)]

        Command --> CommandHandler
        Query --> QueryHandler
        CommandHandler --> WriteDB
        QueryHandler --> ReadDB
        QueryHandler -.->|Enrich| DiscountService[Discount Service<br/>gRPC]
    end

    style Command fill:#ffcdd2
    style Query fill:#c5e1a5
    style CommandHandler fill:#ffcdd2
    style QueryHandler fill:#c5e1a5
```

---

## Comment utiliser ces diagrammes

### Dans GitHub/GitLab
Les fichiers `.md` avec code Mermaid s'affichent automatiquement dans les README.

### Dans VSCode
Installez l'extension **"Markdown Preview Mermaid Support"**

### En ligne
Copiez le code dans:
- https://mermaid.live/
- https://mermaid.ink/

### Dans la documentation
Ces diagrammes peuvent être intégrés dans:
- Confluence
- Notion
- Documentation technique
- Rapports de projet


# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# From repo root — always run in this order
dotnet restore
dotnet build   # Expect 6 nullability warnings in HiveSpace.Domain.Shared; non-blocking

# Run individual services
cd src/HiveSpace.ApiGateway/HiveSpace.YarpApiGateway && dotnet run      # https://localhost:5000, no DB
cd src/HiveSpace.UserService/HiveSpace.UserService.Api && dotnet run    # https://localhost:5001, requires SQL Server
cd src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api && dotnet run  # requires SQL Server + RabbitMQ + Kafka
cd src/HiveSpace.OrderService/HiveSpace.OrderService.Api && dotnet run  # https://localhost:5002, requires SQL Server + RabbitMQ + Kafka

# EF Core migrations (run from project root, targeting the Infrastructure project)
dotnet ef migrations add <Name> --project src/HiveSpace.OrderService/HiveSpace.OrderService.Infrastructure --startup-project src/HiveSpace.OrderService/HiveSpace.OrderService.Api
dotnet ef database update --project src/HiveSpace.OrderService/HiveSpace.OrderService.Infrastructure --startup-project src/HiveSpace.OrderService/HiveSpace.OrderService.Api
```

No test projects exist — `dotnet test` returns immediately.

**Infrastructure requirements**: SQL Server on `localhost:1433` (sa/Passw0rd123!), RabbitMQ on `localhost:5672` (guest/guest), Kafka on `localhost:9092`.

Expected startup warnings: Duende IdentityServer license and MediatR license reminders — development use is permitted.

## Architecture

Clean Architecture / DDD. Each service has four layers: `Domain → Application → Infrastructure → Api`.

```
src/
├── HiveSpace.ApiGateway/HiveSpace.YarpApiGateway/     # YARP reverse proxy, no DB
├── HiveSpace.UserService/                              # Identity & auth (Duende IdentityServer)
├── HiveSpace.CatalogService/                           # Product catalog
├── HiveSpace.OrderService/                             # Orders, cart, coupons, checkout saga
└── HiveSpace.MediaService/                             # Media/file handling (Azure Functions + API)
libs/
├── HiveSpace.Core/                                     # Exceptions, filters, helpers, pagination models
├── HiveSpace.Domain.Shared/                            # AggregateRoot, Entity, ValueObject, IDomainEvent
├── HiveSpace.Application.Shared/                       # ICommand, IQuery, ICommandHandler, IQueryHandler
├── HiveSpace.Infrastructure.Messaging/                 # MassTransit/Kafka/RabbitMQ abstractions
├── HiveSpace.Infrastructure.Messaging.Shared/          # Cross-service saga contracts (CheckoutSaga commands/events)
├── HiveSpace.Infrastructure.Authorization/             # HiveSpaceAuthorizeAttribute, policy helpers
└── HiveSpace.Infrastructure.Persistence/               # Outbox, idempotence, EF interceptors, transaction service
```

All packages are centrally versioned in `Directory.Packages.props` — never specify versions in `.csproj` files.

## Two Patterns for Feature Implementation

### Pattern 1 — Command / Write operations (domain-first)

Used for all write operations and reads that require business rule validation.

1. **Domain**: Define or update aggregate + business methods + repository interface
2. **Application**: `record MyCommand(...) : IRequest<MyResult>` + handler + FluentValidation validator
3. **Infrastructure**: Repository implementation + `IEntityTypeConfiguration<T>` in `EntityConfigurations/`
4. **Api**: Map endpoint or add controller action

### Pattern 2 — Complex query operations (bypass domain)

Used for paginated lists and reporting reads that don't need domain logic. Bypasses EF Core for performance using Dapper.

1. **Application**: Define `IXxxDataQuery` interface + request/response types
2. **Infrastructure**: Implement with `Dapper` + raw SQL in `DataQueries/`
3. **Api**: Call via application service or directly via `ISender`

## API Layer

**UserService** uses MVC controllers:
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UsersController : ControllerBase { ... }
```

**OrderService** uses minimal API endpoints via static extension methods (Carter-style):
```csharp
// src/HiveSpace.OrderService/HiveSpace.OrderService.Api/Endpoints/OrderEndpoints.cs
public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/orders/checkout", async (...) => { ... })
           .RequireAuthorization()
           .WithName("InitiateCheckout")
           .WithTags("Order");
        return app;
    }
}
```

Use `HiveSpaceAuthorizeAttribute.Seller.Policy` for seller-only endpoints.

## Checkout Saga (MassTransit)

The checkout flow is a distributed saga orchestrated via MassTransit. Contracts live in `libs/HiveSpace.Infrastructure.Messaging.Shared/CheckoutSaga/`.

- **Trigger**: POST `/api/v1/orders/checkout` publishes `CheckoutInitiated` with a `CorrelationId`
- **Saga state**: `CheckoutSagaState` persisted via EF Core
- **Key events**: `ValidationCompleted/Failed → InventoryReserved/Failed → OrderCreated → OrderMarkedAsCOD → SellersNotified → PackageConfirmed/Rejected → InventoryConfirmed → CustomerNotified`
- Compensation events (`ReleaseInventory`, `CancelOrder`) handle failures

When publishing saga events from endpoints, always call `await db.SaveChangesAsync(ct)` after publishing to persist outbox records in the same transaction.

## Coding Rules

### Error handling — CRITICAL
Always use exceptions from `HiveSpace.Domain.Shared.Exceptions`. Never use `System.ArgumentException`, `System.InvalidOperationException`, etc. in domain or application code.

```csharp
// Available types (all inherit DomainException):
throw new NotFoundException(OrderDomainErrorCode.OrderNotFound, nameof(Order));      // 404
throw new InvalidFieldException(OrderDomainErrorCode.InvalidAmount, nameof(amount)); // 400
throw new ConflictException(OrderDomainErrorCode.CouponAlreadyUsed, nameof(Coupon)); // 409
throw new ForbiddenException(OrderDomainErrorCode.NotOrderOwner, nameof(Order));     // 403
// Always use nameof() for the source parameter
```

### Entity configuration
- Place in `[Service].Infrastructure/EntityConfigurations/[Entity]EntityConfiguration.cs`
- Table names must be `snake_case` (e.g., `builder.ToTable("order_packages")`)
- Apply via `builder.ApplyConfigurationsFromAssembly()`

### DTOs
Use C# `record` types for all DTOs. FluentValidation validators use `.WithState(_ => new Error(ErrorCode, nameof(field)))`.

### Commands and queries
Commands implement `IRequest<TResult>` (MediatR). Query handlers for complex reads implement `IQueryHandler<TQuery, TResult>` from `HiveSpace.Application.Shared`.

### Domain events to integration events
Domain events raised via `AggregateRoot.RaiseDomainEvent()` are captured by `DomainEventToOutboxInterceptor` and written to the outbox table. The outbox processor publishes them as integration events via MassTransit.

### Monetary values
Monetary amounts are stored as `long` (e.g., cents/smallest currency unit) with a separate currency `string` column — do not use `decimal` for money.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Tech Stack

- .NET 8, ASP.NET Core Minimal APIs or Controller
- Entity Framework Core 8
- Mediator for CQRS pattern (source-generated) or using interface with implementation
- FluentValidation for request validation
- Scalar for API documentation (OpenAPI)

## Plan First

- Treat any non-trivial task as architectural work.
- Inspect the existing layer before changing it.
- Keep the generated solution minimal, coherent, and production-oriented.

## Always Clarify Implementation Pattern Before Starting

Before implementing any new feature — whether in plan mode or not — always ask which pattern to use for the Application layer:

**Option A — CQRS (MediatR)**
Each operation is a discrete command or query handler. Use this when the feature maps cleanly to a single intent (create, update, cancel, list, get).

```text
Application/
  [Feature]/
    Commands/
      [Action][Entity]Command.cs
      [Action][Entity]CommandHandler.cs
      [Action][Entity]Validator.cs
    Queries/
      [Get][Entity]Query.cs
      [Get][Entity]QueryHandler.cs
```

**Option B — Service / Interface**
A service class groups related operations behind an interface. Use this when multiple operations share state, context, or helpers that would be awkward to repeat across individual handlers.

```text
Application/
  Interfaces/
    I[Feature]Service.cs
  Services/
    [Feature]Service.cs
```

**When to ask:** Always — even for small features. The two patterns are not equivalent and mixing them without intent creates inconsistency. If the existing service already uses one pattern for the same aggregate, follow it unless there is a clear reason to diverge.

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
├── HiveSpace.Infrastructure.Messaging.Shared/          # Cross-service saga contracts (commands/events)
├── HiveSpace.Infrastructure.Authorization/             # HiveSpaceAuthorizeAttribute, policy helpers
└── HiveSpace.Infrastructure.Persistence/               # Outbox, idempotence, EF interceptors, transaction service
```

All packages are centrally versioned in `Directory.Packages.props` — never specify versions in `.csproj` files.

## Service Architecture

Services in this repo follow one of two structural patterns.

### Full Clean Architecture + Strict DDD

Used by: **UserService**, **CatalogService**, **OrderService**

Four projects per service with hard layer boundaries:

```text
[Service].Domain/           # Aggregates, value objects, domain services, repository interfaces
[Service].Application/      # MediatR commands/queries/handlers, FluentValidation validators, data query interfaces
[Service].Infrastructure/   # EF Core repo implementations, Dapper data queries, messaging publishers
[Service].Api/              # Controllers or minimal endpoints, MassTransit saga state machines, consumers
```

Rules:

- All business logic lives in Domain aggregates and domain services
- Application layer orchestrates — no business rules here
- Infrastructure never referenced from Domain or Application (dependency inversion)
- Saga state machines and their consumers live in the Api project under `Api/Sagas/` and `Api/Consumers/`

### Lightweight / Non-DDD

Used by: **MediaService**

One to three projects — typically a Core project plus an Api project, and optionally additional host projects (e.g. Azure Functions):

```text
[Service].Core/    # Domain models, interfaces, service classes, validators — all in one place
[Service].Api/     # Controllers, DI wiring
[Service].Func/    # Optional: additional host (Azure Functions, workers, etc.)
```

Rules:

- No strict Application/Infrastructure split required
- Business logic lives in service classes rather than domain aggregates
- Entities still use **private setters** — no public property mutation from outside the class
- EF configurations, storage logic, etc. can live directly in Core

## Two Patterns for Feature Implementation

### Pattern 1 — Command / Write operations (domain-first)

Used for all write operations and reads that require business rule validation.

1. **Domain**: Define or update aggregate + business methods + repository interface
2. **Application**: `record MyCommand(...) : ICommand<MyResult>` + handler + FluentValidation validator (use `ICommand`/`ICommandHandler` from `HiveSpace.Application.Shared`)
3. **Infrastructure**: Repository implementation + `IEntityTypeConfiguration<T>` in `EntityConfigurations/`
4. **Api**: Map endpoint or add controller action

### Pattern 2 — Complex query operations (bypass domain)

Used for paginated lists and reporting reads that don't need domain logic. Bypasses EF Core for performance using Dapper.

1. **Application**: Define `IXxxDataQuery` interface + request/response types
2. **Infrastructure**: Implement with `Dapper` + raw SQL in `DataQueries/`
3. **Api**: Call via application service or directly via `ISender`

## DDD Building Blocks

### Domain Services

For complex cross-entity business logic that doesn't belong on a single aggregate, implement `IDomainService`:

```csharp
public class UserManager : IDomainService
{
    public async Task<User> RegisterUserAsync(Email email, string userName, ...)
    {
        await CanUserBeRegisteredAsync(email, userName, cancellationToken);
        return User.Create(email, userName, fullName);
    }
}
// Register via AddAppDomainServices() in DI wiring
```

### Value Objects

Immutable types where equality is based on value, not identity. Validate in the constructor and throw `InvalidFieldException` on bad input.

```csharp
public sealed class DeliveryAddress : ValueObject
{
    public string RecipientName { get; }
    public string PhoneNumber { get; }
    // ... all init in constructor, no public setters
}
```

Examples in OrderService: `DeliveryAddress`, `ProductSnapshot`, `PhoneNumber`, `PackageDimensions`

### Specifications

Used for named, composable query predicates on aggregates:

```csharp
public class CouponOngoingSpecification : Specification<Coupon> { ... }
public class CouponOwnedByStoreSpecification : Specification<Coupon> { ... }
```

Place specifications alongside the aggregate in the Domain layer.

### IUserContext

Inject `IUserContext` in command/query handlers to access the authenticated user:

```csharp
private readonly IUserContext _userContext;
// Provides: _userContext.UserId, _userContext.StoreId, etc.
```

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

## Sagas (MassTransit)

Sagas are distributed workflows orchestrated via MassTransit state machines. A service can have multiple sagas. Contracts (commands and events) are shared via `libs/HiveSpace.Infrastructure.Messaging.Shared/`.

**File placement within a service:**

```text
Api/Sagas/[SagaName]/[SagaName]StateMachine.cs   # State machine definition
Api/Sagas/[SagaName]/[SagaName]State.cs          # or in Infrastructure/Sagas/
Api/Consumers/Saga/[SagaName]/[Step]Consumer.cs  # One consumer per saga step
```

**Two communication styles used inside sagas:**

| Style             | When to use                                           | Example                                 |
| ----------------- | ----------------------------------------------------- | --------------------------------------- |
| Request/Response  | Synchronous saga step; needs a reply to advance state | `OrderCreation`, `InventoryReservation` |
| Publish/Subscribe | Async notification or compensation; fire and forget   | `ReleaseInventory`, `CancelOrder`       |

Request/Response steps should always define a timeout (e.g. 30 minutes). Compensation events are published when a step fails and trigger rollback in upstream services.

**Example — checkout flow (for reference):**

- Trigger: POST `/api/v1/orders/checkout` publishes `CheckoutInitiated` with a `CorrelationId`
- Saga state: `CheckoutSagaState` persisted via EF Core
- Steps (request/response): OrderCreation → InventoryReservation → CODMarking → CartClearing
- Compensation: `ReleaseInventory`, `CancelOrder` on failure

Always call `await db.SaveChangesAsync(ct)` after publishing to persist outbox records in the same transaction.

## Coding Rules

## Git Commit Guardrails

- Agents must never stage or commit any `*.json` file.
- If a task changes one or more `*.json` files, agents must ask the user to add/stage those JSON files manually.
- Agents can stage and commit only non-JSON files after user confirmation that JSON staging is handled.
- After finishing a task, agents must delete temporary files they created (for example: ad-hoc error logs, scratch/debug files, or one-off investigation artifacts) unless the user explicitly asks to keep them.
- Agents must not stage or commit temporary files created only for debugging or task tracking.

### Error handling — CRITICAL

Always use exceptions from `HiveSpace.Domain.Shared.Exceptions`. Never use `System.ArgumentException`, `System.InvalidOperationException`, etc. in domain or application code.

```csharp
// Available types (all inherit DomainException):
throw new NotFoundException(OrderDomainErrorCode.OrderNotFound, nameof(Order));      // 404
throw new InvalidFieldException(OrderDomainErrorCode.InvalidAmount, nameof(amount)); // 400
throw new ConflictException(OrderDomainErrorCode.CouponAlreadyUsed, nameof(Coupon)); // 409
throw new ForbiddenException(OrderDomainErrorCode.NotOrderOwner, nameof(Order));     // 403
// Always use nameof() for the source parameter — never hardcoded strings
```

**Extending exceptions** — optional, for descriptive domain-specific types:

```csharp
// ✅ Extend HiveSpace base exceptions when a named type adds clarity
public class InvalidEmailException : DomainException
{
    public InvalidEmailException() : base(400, UserDomainErrorCode.InvalidEmail, nameof(Email)) { }
}

// ❌ Never extend System exceptions
public class InvalidUserException : ArgumentException { }  // WRONG
```

**Defining domain error codes:**

```csharp
// Each service defines its own error codes extending DomainErrorCode
public class OrderDomainErrorCode : DomainErrorCode
{
    private OrderDomainErrorCode(int id, string name, string code) : base(id, name, code) { }
    public static readonly OrderDomainErrorCode OrderNotFound = new(2001, "OrderNotFound", "ORD2001");
    public static readonly OrderDomainErrorCode InvalidAmount  = new(2002, "InvalidAmount",  "ORD2002");
}
```

### Commands — always go through the domain

```csharp
// ✅ Load aggregate → call domain method → save
var order = await _repo.GetByIdAsync(id)
    ?? throw new NotFoundException(OrderDomainErrorCode.OrderNotFound, nameof(Order));
order.Cancel();
await _repo.SaveChangesAsync(ct);

// ❌ Never update EF entities directly — bypasses domain validation
await _dbContext.Orders.Where(o => o.Id == id)
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Cancelled));
```

### Entity configuration

- Place in `[Service].Infrastructure/EntityConfigurations/[Entity]EntityConfiguration.cs`
- Table names must be `snake_case` (e.g., `builder.ToTable("order")`)
- Apply via `builder.ApplyConfigurationsFromAssembly()`

### DTOs

Use C# `record` types for all DTOs. FluentValidation validators use `.WithState(_ => new Error(ErrorCode, nameof(field)))`:

```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateAdminRequestDto.Email)))
    .EmailAddress()
    .WithState(_ => new Error(UserDomainErrorCode.InvalidEmail, nameof(CreateAdminRequestDto.Email)));
```

### Commands and queries

Commands implement `ICommand<TResult>` from `HiveSpace.Application.Shared` (wraps MediatR `IRequest<TResult>`). Query handlers for complex reads implement `IQueryHandler<TQuery, TResult>` from `HiveSpace.Application.Shared`.

### Domain events to integration events

Domain events raised via `AggregateRoot.RaiseDomainEvent()` are captured by `DomainEventToOutboxInterceptor` and written to the outbox table. The outbox processor publishes them as integration events via MassTransit.

### Monetary values

Monetary amounts are stored as `long` (e.g., cents/smallest currency unit) with a separate currency `string` column — do not use `decimal` for money.

### Primary constructors

Prefer C# 12 primary constructors for dependency injection — no explicit field declarations needed:

```csharp
// ✅ DO
public class OrderService(IOrderRepository repo, IUserContext userContext) { }

// ❌ DON'T
public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) { _repo = repo; }
}
```

### Dependency injection lifetime

Always register services, repositories, and data queries as `AddScoped`. Only deviate with a clear reason:

```csharp
// ✅
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IOrderRepository, SqlOrderRepository>();
services.AddScoped<IOrderDataQuery, OrderDataQuery>();

// ❌ Don't use AddSingleton for stateful/db-dependent services
```

### Async / await

Never block on async code — always `await`:

```csharp
// ✅
var order = await _repo.GetByIdAsync(id, ct);

// ❌ Deadlock risk
var order = _repo.GetByIdAsync(id).Result;
```

### Error code naming convention

Use a domain prefix so error codes are traceable across services:

| Scope          | Prefix    | Example                                              |
| -------------- | --------- | ---------------------------------------------------- |
| Shared/common  | `APP0xxx` | `APP0004` — in `HiveSpace.Core` as `CommonErrorCode` |
| UserService    | `USR1xxx` | `USR1001` — `UserDomainErrorCode`                    |
| OrderService   | `ORD2xxx` | `ORD2001` — `OrderDomainErrorCode`                   |
| CatalogService | `CAT3xxx` | `CAT3001` — `CatalogDomainErrorCode`                 |

### One type per file

Each file contains exactly one primary type. File name must match the type name:

```text
CreateOrderCommand.cs          → record CreateOrderCommand
CreateOrderCommandHandler.cs   → class CreateOrderCommandHandler
CreateOrderValidator.cs        → class CreateOrderValidator
```

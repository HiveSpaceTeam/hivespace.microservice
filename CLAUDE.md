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
cd src/HiveSpace.ApiGateway/HiveSpace.YarpApiGateway && dotnet run      # http://localhost:5000, no DB
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
├── HiveSpace.PaymentService/                           # Payment processing
├── HiveSpace.MediaService/                             # Media/file handling (Azure Blob + Functions)
└── HiveSpace.NotificationService/                      # Notifications (email, in-app, SignalR)
libs/
├── HiveSpace.Core/                                     # Exceptions, filters, helpers, pagination models
├── HiveSpace.Domain.Shared/                            # AggregateRoot, Entity, ValueObject, IDomainEvent
├── HiveSpace.Application.Shared/                       # ICommand, IQuery, ICommandHandler, IQueryHandler
├── HiveSpace.Infrastructure.Messaging/                 # MassTransit/Kafka/RabbitMQ abstractions
├── HiveSpace.Infrastructure.Messaging.Shared/          # Cross-service saga contracts (commands/events)
├── HiveSpace.Infrastructure.Authorization/             # HiveSpaceAuthorizeAttribute, policy helpers
└── HiveSpace.Infrastructure.Persistence/               # Idempotence, EF interceptors (audit, soft-delete), transaction service
```

All packages are centrally versioned in `Directory.Packages.props` — never specify versions in `.csproj` files.

## Service Architecture

Services follow one of two archetypes. **Identify the archetype before any new service or feature work.**

### Which archetype?

| Signal | Full Service | Lite Service |
|--------|-------------|-------------|
| Owns business aggregates with lifecycle rules | ✅ | — |
| Participates in a distributed saga or workflow | ✅ | — |
| Primarily orchestrates infrastructure (storage, email, notifications) | — | ✅ |
| Narrow feature scope, small operational footprint | — | ✅ |

**Assignments:**
- **Full Service**: UserService, CatalogService, OrderService, PaymentService
- **Lite Service**: MediaService, NotificationService

---

### Full Service

Four projects with hard layer boundaries. Dependency direction: `Domain ← Application ← Infrastructure ← Api`.

```text
[Service].Domain/
  Aggregates/[Root]/
    [Root].cs                    # extends AggregateRoot<TKey>
  Enumerations/
  Exceptions/
    [Service]DomainErrorCode.cs  # extends DomainErrorCode — REQUIRED
  Repositories/
    I[Root]Repository.cs
  ValueObjects/                  # optional

[Service].Application/
  [Feature]/                     # FLEXIBLE — CQRS or Service-based (see below)
  Interfaces/
    Messaging/
      I[Service]EventPublisher.cs

[Service].Infrastructure/
  Data/
    [Service]DbContext.cs
  EntityConfigurations/
    [Entity]EntityConfiguration.cs
  Repositories/
    Sql[Root]Repository.cs
  DataQueries/                   # optional — only when Dapper reads are used
  Messaging/
    Publishers/
      [Service]EventPublisher.cs
  Sagas/                         # saga state persistence (optional)
  Migrations/

[Service].Api/
  # FLEXIBLE — Controllers or Minimal Endpoints (see below)
  Consumers/
    Saga/[SagaName]/
    Sync/
  Sagas/
    [SagaName]/[SagaName]StateMachine.cs
  Extensions/
    HostingExtensions.cs
    ServiceCollectionExtensions.cs
  Program.cs
```

**Mandatory rules:**
- Domain has zero references to Application, Infrastructure, or Api
- Application references only Domain and shared libs — never Infrastructure
- `[Service]DomainErrorCode` required in `Domain/Exceptions/`
- All entities: private setters — no public mutation from outside the class
- Write path: load aggregate → call domain method → save (never `ExecuteUpdateAsync`)
- `AddScoped` for all repositories, services, and data queries
- Every command/query with user input must have a validator; register all validators + `ValidationPipelineBehavior<,>` in `AddMediatR` — see **Validation pipeline** rule in Coding Rules

**Flexible decision points — agent must ask before implementing:**

| Decision | Option A | Option B | Rule |
|----------|----------|----------|------|
| Application layer | **CQRS** — `ICommand`/`IQuery` + handlers per operation | **Service-based** — `I[Feature]Service` + `[Feature]Service` | Follow what the aggregate already uses; ask if first feature |
| API surface | **MVC Controllers** — `[Feature]Controller : ControllerBase` | **Minimal Endpoints** — static `Map[Feature]Endpoints()` | Follow existing service convention; ask if new service |
| Complex reads | **EF Core only** — via repository | **Dapper hybrid** — `I[Feature]DataQuery` interface + `DataQueries/` impl | Use Dapper only for paginated list / reporting queries |

**CQRS layout (when chosen):**

```text
Application/[Feature]/
  Commands/
    [Action][Entity]/
      [Action][Entity]Command.cs
      [Action][Entity]CommandHandler.cs
      [Action][Entity]Validator.cs
  Queries/
    [Get][Entity]/
      [Get][Entity]Query.cs
      [Get][Entity]QueryHandler.cs
  Dtos/
```

**Service-based layout (when chosen):**

```text
Application/
  Interfaces/
    I[Feature]Service.cs
  Services/
    [Feature]Service.cs
  Validators/
    [Request]Validator.cs
  Dtos/
```

**New service checklist — Full:**
1. Run from repo root: `.\scripts\new-service.ps1 -ServiceName HiveSpace.[Name]Service -TemplateName ms-full -AddToSolution`
2. Create `Domain/Exceptions/[Name]DomainErrorCode.cs` (choose a unique error code prefix — see table in Coding Rules)
3. Wire DI in `Extensions/ServiceCollectionExtensions.cs` and `Extensions/HostingExtensions.cs`
4. Run first migration: `dotnet ef migrations add InitialCreate --project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Infrastructure --startup-project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Api`

---

### Lite Service

Two projects required; a third host project is optional.

```text
[Service].Core/
  Features/
    [Feature]/                    # one folder per bounded concern
      Commands/
        [Action][Entity]/
          [Action][Entity]Command.cs
          [Action][Entity]CommandHandler.cs
          [Action][Entity]Validator.cs
      Queries/
        [Get][Entity]/
          [Get][Entity]Query.cs
          [Get][Entity]QueryHandler.cs
      Dtos/                       # feature-specific response types
  DomainModels/
    [Entity].cs                   # private setters required
    Enum/
    External/                     # cross-service read-only refs (optional)
  Exceptions/
    [Service]DomainErrorCode.cs   # REQUIRED
  Interfaces/                     # cross-feature: repo + infra interfaces
  Services/                       # cross-cutting impls (dedup, rate-limit, template renderer, etc.)
  Persistence/
    [Service]DbContext.cs
    EntityConfigurations/
      [Entity]EntityConfiguration.cs
    Repositories/
      [Entity]Repository.cs
    Migrations/
    SeedData/                     # optional
  Infrastructure/                 # external integrations: Azure, email, channel providers, config
    [Provider]/
  BackgroundJobs/                 # Hangfire jobs (optional)
  Dispatch/                       # optional — pipeline + router + internal models

[Service].Api/
  Endpoints/
    [Feature]Endpoints.cs         # static Map[Feature]Endpoints() extension methods
  Consumers/                      # MassTransit consumers (if service listens to events)
  Hubs/                           # SignalR hubs (optional)
  Extensions/
    HostingExtensions.cs
    ServiceCollectionExtensions.cs
  Program.cs

[Service].Func/                   # OPTIONAL — only for a genuinely separate function/worker host
  Functions/
  Program.cs
```

**Mandatory rules:**
- `[Service]DomainErrorCode` required in `Core/Exceptions/`
- All entities: private setters — no public mutation from outside the class
- `AddScoped` for all handlers and repositories
- Exceptions: `HiveSpace.Domain.Shared.Exceptions` types only — never `System.*`
- CQRS (`ICommand`/`IQuery` + handlers) for all user-facing operations — no `I[Feature]Service` pattern for Lite Services
- Handlers always inject repositories — never `DbContext` directly
- Cross-cutting infrastructure services (pipeline, router, dedup, rate-limiter) use named interfaces in `Interfaces/` — not exposed directly by endpoints
- Always Minimal Endpoints — no MVC Controllers
- Always EF Core only — no Dapper
- Every command/query with user input must have a validator; register all validators + `ValidationPipelineBehavior<,>` in `AddMediatR` — see **Validation pipeline** rule in Coding Rules
- Add `.Func` project only when a genuinely separate process host is required; background jobs belong in Hangfire inside `Core/BackgroundJobs/`

**New service checklist — Lite:**
1. Run from repo root: `.\scripts\new-service.ps1 -ServiceName HiveSpace.[Name]Service -TemplateName ms-lite -AddToSolution`
2. Create `Core/Exceptions/[Name]DomainErrorCode.cs` (choose a unique error code prefix)
3. Wire DI in `Extensions/ServiceCollectionExtensions.cs` and `Extensions/HostingExtensions.cs`
4. Run first migration: `dotnet ef migrations add InitialCreate --project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Core --startup-project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Api`

## Feature Implementation — Full Services

> Applies to **Full Services** only. Lite Services always use the service-based approach: define an interface in `Core/Interfaces/`, implement it in `Core/Services/`, and call it from the controller.

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

> Applies to **Full Services** only.

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

## Sagas (MassTransit)

> Applies to **Full Services** only.

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

## Linked documentation files

`CLAUDE.md` and `AGENTS.md` are kept in sync by a PostToolUse hook (`.claude/hooks/sync-docs.sh`). After editing either file, you **must** update the other:
- Changes to `## Service Architecture` in CLAUDE.md → update the service table and hard rules in AGENTS.md
- Changes to the service table or rules in AGENTS.md → update `## Service Architecture` in CLAUDE.md

## PR process

Never run `gh pr create` directly. A PreToolUse hook (`.claude/hooks/guard-pr.sh`) blocks it. Required flow:
1. Run `bash scripts/sync-config.sh` to sync all `appsettings.json` / `local.settings.json` to `hivespace.config/`
2. Run `npx gitnexus analyze` to sync the GitNexus index with current changes
3. Tell the user to **start a new session** in this repository
4. In the new session, run `/review` to review all current changes
5. Apply any fixes from the review
6. Only then: `gh pr create`

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

### Validation pipeline — MANDATORY for all services

Every service that uses CQRS **must** register `ValidationPipelineBehavior<,>` from `HiveSpace.Application.Shared.Behaviors` as an open MediatR pipeline behavior. This runs all registered `IValidator<TRequest>` automatically before the handler executes.

```csharp
// In AddMediatR / AddAppMediatR:
services.AddValidatorsFromAssemblyContaining<MyCommand>(); // register all validators in the assembly
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<MyCommand>();
    cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>)); // runs validators automatically
});
```

**Validator rules:**
- Every command or query that accepts user input **must** have a corresponding `[Name]Validator` file in the same feature folder
- Validators use `.WithState(_ => new Error(ErrorCode, nameof(field)))` — never `.WithMessage()`
- Use `CommonErrorCode.Required` for empty/null checks, `CommonErrorCode.InvalidPageNumber`/`InvalidPageSize` for pagination, `CommonErrorCode.InvalidArgument` for range/format errors
- Commands or queries with no user-controlled parameters (e.g. `GetUnreadCountQuery()`) do not need validators

### Integration event publishing

Publish integration events directly from command/service handlers via `I[Service]EventPublisher`. Always call the publisher **before** `SaveChangesAsync()` so MassTransit's bus outbox writes the message to the DB in the same transaction as your domain data.

```csharp
// ✅ Correct — publisher before save; MassTransit bus outbox commits both atomically
await eventPublisher.PublishSomethingAsync(aggregate, ct);
await repository.SaveChangesAsync(ct);

// ❌ Wrong — event lost if service crashes, or published before DB commit
await repository.SaveChangesAsync(ct);
await eventPublisher.PublishSomethingAsync(aggregate, ct);
```

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

| Scope               | Prefix    | Example                                              |
| ------------------- | --------- | ---------------------------------------------------- |
| Shared/common       | `APP0xxx` | `APP0004` — in `HiveSpace.Core` as `CommonErrorCode` |
| UserService         | `USR1xxx` | `USR1001` — `UserDomainErrorCode`                    |
| OrderService        | `ORD2xxx` | `ORD2001` — `OrderDomainErrorCode`                   |
| CatalogService      | `CAT3xxx` | `CAT3001` — `CatalogDomainErrorCode`                 |
| NotificationService | `NTF4xxx` | `NTF4001` — `NotificationDomainErrorCode`            |
| MediaService        | `MED1xxx` | `MED1001` — `MediaDomainErrorCode`                   |

### One type per file

Each file contains exactly one primary type. File name must match the type name:

```text
CreateOrderCommand.cs          → record CreateOrderCommand
CreateOrderCommandHandler.cs   → class CreateOrderCommandHandler
CreateOrderValidator.cs        → class CreateOrderValidator
```

### IUserContext — User Identity in Handlers

`IUserContext` (from `HiveSpace.Core.Contexts`) is the authoritative source for the authenticated user's identity in HTTP-facing code. Registration is handled by `AddCoreServices()` — never register it manually.

**Inject it in every HTTP-facing handler that needs user identity:**

```csharp
public class MyCommandHandler(IMyRepository repo, IUserContext userContext)
    : ICommandHandler<MyCommand>
{
    public async Task Handle(MyCommand request, CancellationToken cancellationToken)
    {
        // Identity
        var userId = userContext.UserId;       // Guid — from JWT "sub" claim
        var email  = userContext.Email;        // string
        var name   = userContext.FullName;     // string? — JWT "name" claim, may be null
        var locale = userContext.Locale;       // string? — JWT "locale" claim, may be null

        // Role helpers — prefer these over raw Roles list
        if (userContext.IsSeller)      { /* seller logic */ }
        if (userContext.IsBuyer)       { /* buyer logic */ }
        if (userContext.IsAdmin)       { /* admin logic */ }
        if (userContext.IsSystemAdmin) { /* sysadmin logic */ }
        var storeId = userContext.StoreId;     // Guid? — null for non-sellers
    }
}
```

**Always prefer typed boolean helpers over raw role strings:**

```csharp
// ✅ Use typed helpers
if (userContext.IsSeller) { ... }

// ❌ Avoid raw role string parsing
if (userContext.Roles.FirstOrDefault() == "Seller") { ... }
```

**`FullName` and `Locale` — JWT-first, UserRef fallback:**

```csharp
// Use IUserContext for the current user — no DB round-trip
var name   = userContext.FullName ?? "there";     // personalise templates
var locale = userContext.Locale   ?? "en";        // template language

// Use IUserRefRepository only for other users (e.g., dispatch consumers)
// or when avatar URL / store name is needed (not in JWT)
var ref = await userRefRepo.GetByIdAsync(targetUserId, ct);
```

**Do NOT inject `IUserContext` in these scopes — there is no HTTP context:**

| Scope | Correct alternative |
|-------|-------------------|
| MassTransit consumers | Read user identity from the message payload |
| Hangfire background jobs | Pass user data as job arguments at enqueue time |
| SignalR hubs | Use `Context.User` (ClaimsPrincipal captured at handshake) and `Context.UserIdentifier` (set by `IUserIdProvider`) |

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **hivespace.microservice** (6535 symbols, 16144 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## When Debugging

1. `gitnexus_query({query: "<error or symptom>"})` — find execution flows related to the issue
2. `gitnexus_context({name: "<suspect function>"})` — see all callers, callees, and process participation
3. `READ gitnexus://repo/hivespace.microservice/process/{processName}` — trace the full execution flow step by step
4. For regressions: `gitnexus_detect_changes({scope: "compare", base_ref: "main"})` — see what your branch changed

## When Refactoring

- **Renaming**: MUST use `gitnexus_rename({symbol_name: "old", new_name: "new", dry_run: true})` first. Review the preview — graph edits are safe, text_search edits need manual review. Then run with `dry_run: false`.
- **Extracting/Splitting**: MUST run `gitnexus_context({name: "target"})` to see all incoming/outgoing refs, then `gitnexus_impact({target: "target", direction: "upstream"})` to find all external callers before moving code.
- After any refactor: run `gitnexus_detect_changes({scope: "all"})` to verify only expected files changed.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Tools Quick Reference

| Tool | When to use | Command |
|------|-------------|---------|
| `query` | Find code by concept | `gitnexus_query({query: "auth validation"})` |
| `context` | 360-degree view of one symbol | `gitnexus_context({name: "validateUser"})` |
| `impact` | Blast radius before editing | `gitnexus_impact({target: "X", direction: "upstream"})` |
| `detect_changes` | Pre-commit scope check | `gitnexus_detect_changes({scope: "staged"})` |
| `rename` | Safe multi-file rename | `gitnexus_rename({symbol_name: "old", new_name: "new", dry_run: true})` |
| `cypher` | Custom graph queries | `gitnexus_cypher({query: "MATCH ..."})` |

## Impact Risk Levels

| Depth | Meaning | Action |
|-------|---------|--------|
| d=1 | WILL BREAK — direct callers/importers | MUST update these |
| d=2 | LIKELY AFFECTED — indirect deps | Should test |
| d=3 | MAY NEED TESTING — transitive | Test if critical path |

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/hivespace.microservice/context` | Codebase overview, check index freshness |
| `gitnexus://repo/hivespace.microservice/clusters` | All functional areas |
| `gitnexus://repo/hivespace.microservice/processes` | All execution flows |
| `gitnexus://repo/hivespace.microservice/process/{name}` | Step-by-step execution trace |

## Self-Check Before Finishing

Before completing any code modification task, verify:
1. `gitnexus_impact` was run for all modified symbols
2. No HIGH/CRITICAL risk warnings were ignored
3. `gitnexus_detect_changes()` confirms changes match expected scope
4. All d=1 (WILL BREAK) dependents were updated

## Keeping the Index Fresh

After committing code changes, the GitNexus index becomes stale. Re-run analyze to update it:

```bash
npx gitnexus analyze
```

If the index previously included embeddings, preserve them by adding `--embeddings`:

```bash
npx gitnexus analyze --embeddings
```

To check whether embeddings exist, inspect `.gitnexus/meta.json` — the `stats.embeddings` field shows the count (0 means no embeddings). **Running analyze without `--embeddings` will delete any previously generated embeddings.**

> Claude Code users: A PostToolUse hook handles this automatically after `git commit` and `git merge`.

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->

# Service Architecture

Services follow one of two archetypes. **Identify the archetype before any new service or feature work.**

## Which archetype?

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

## Full Service

Four projects with hard layer boundaries. Dependency direction: `Domain ← Application ← Infrastructure ← Api`.

```text
[Service].Domain/
  Aggregates/[Root]/
    [Root].cs                    # extends AggregateRoot<TKey>
  Enumerations/
  Exceptions/
    [Service]DomainErrorCode.cs  # extends DomainErrorCode — REQUIRED
  External/                      # optional — cross-service read-only reference entities (no lifecycle rules)
  Repositories/
    I[Root]Repository.cs
  ValueObjects/                  # optional

[Service].Application/
  [Feature]/                     # FLEXIBLE — CQRS or Service-based (see below)
    Commands/[Action][Entity]/   # CQRS: Command.cs + Handler.cs + Validator.cs
    Queries/[Get][Entity]/       # CQRS: Query.cs + Handler.cs
    Dtos/                        # Response/request types scoped to this feature
    Mappers/                     # optional — Domain → DTO object mapping helpers
  Contracts/                     # optional — integration event request/response types shared with Api consumers
  Interfaces/
    Messaging/
      I[Service]EventPublisher.cs

[Service].Infrastructure/
  Data/
    [Service]DbContext.cs
  EntityConfigurations/
    [Entity]EntityConfiguration.cs
  Repositories/
    [DbType][Root]Repository.cs  # prefix = DB technology: Sql, Mongo, Cosmos, etc.
  DataQueries/                   # optional — for reads bypassing the repository
  Messaging/
    Publishers/
      [Service]EventPublisher.cs
  Mappers/                       # optional — ONLY for mappers that bridge Domain ↔ Infrastructure types
  Sagas/                         # saga state persistence (optional)
  SeedData/                      # optional — ISeeder implementations + DataSeeder.cs orchestrator
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
- Every command/query with user input must have a validator; register all validators + `ValidationPipelineBehavior<,>` in `ApplicationServiceCollectionExtensions.AddApplication()` (Application project)
- Repository implementations named `[DbType][Root]Repository` (e.g. `SqlOrderRepository`, `MongoOrderRepository`)
- **Mappers**: Domain → DTO mappers belong in `Application/[Feature]/Mappers/`; mappers that bridge Domain ↔ Infrastructure-specific types (e.g. `ApplicationUser`) belong in `Infrastructure/Mappers/`

**Flexible decision points — agent must ask before implementing:**

| Decision | Option A | Option B | Rule |
|----------|----------|----------|------|
| Application layer | **CQRS** — `ICommand`/`IQuery` + handlers per operation | **Service-based** — `I[Feature]Service` + `[Feature]Service` | Follow what the service already uses (see table below); ask if new service. UserService: Service-based. All others: CQRS. |
| API surface | **MVC Controllers** — `[Feature]Controller : ControllerBase` | **Minimal Endpoints** — static `Map[Feature]Endpoints()` | Follow existing service convention; ask if new service |
| Complex reads | **EF Core only** — via repository | **Dapper hybrid** — `I[Feature]DataQuery` interface + `DataQueries/` impl | Prefer Dapper for complex paginated / reporting reads; EF Core projections are acceptable for simpler cases |

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
  Models/
    Requests/
      [Feature]/
    Responses/
      [Feature]/
  Constant/
    Enum/                        # query-shaping enums (filter values, sort options) — not domain enums
```

> Note: service-based Application layers use a flat `Models/Requests/` + `Models/Responses/` structure instead of per-feature `Dtos/` folders. Validators are called manually in controllers (no MediatR pipeline needed).

**New service checklist — Full:**
1. Run from repo root: `.\scripts\new-service.ps1 -ServiceName HiveSpace.[Name]Service -TemplateName ms-full -AddToSolution`
2. Create `Domain/Exceptions/[Name]DomainErrorCode.cs` (choose a unique error code prefix — see error code table in `coding-rules.md`)
3. Wire DI in `Extensions/ServiceCollectionExtensions.cs` and `Extensions/HostingExtensions.cs`
4. Run first migration: `dotnet ef migrations add InitialCreate --project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Infrastructure --startup-project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Api`

---

## Lite Service

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
    External/                     # optional — cross-service read-only reference entities (no lifecycle rules)
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
- Every command/query with user input must have a validator; register all validators + `ValidationPipelineBehavior<,>` in `ApplicationServiceCollectionExtensions.AddApplication()` (Application project)
- Add `.Func` project only when a genuinely separate process host is required; background jobs belong in Hangfire inside `Core/BackgroundJobs/`

**New service checklist — Lite:**
1. Run from repo root: `.\scripts\new-service.ps1 -ServiceName HiveSpace.[Name]Service -TemplateName ms-lite -AddToSolution`
2. Create `Core/Exceptions/[Name]DomainErrorCode.cs` (choose a unique error code prefix)
3. Wire DI in `Extensions/ServiceCollectionExtensions.cs` and `Extensions/HostingExtensions.cs`
4. Run first migration: `dotnet ef migrations add InitialCreate --project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Core --startup-project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Api`

# Coding Rules

## Error handling — CRITICAL

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

## Commands — always go through the domain

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

## Entity configuration

- Place in `[Service].Infrastructure/EntityConfigurations/[Entity]EntityConfiguration.cs`
- Table names must be `snake_case` (e.g., `builder.ToTable("order")`)
- Apply via `builder.ApplyConfigurationsFromAssembly()`

## DTOs

Use C# `record` types for all DTOs. FluentValidation validators use `.WithState(_ => new Error(ErrorCode, nameof(field)))`:

```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateAdminRequestDto.Email)))
    .EmailAddress()
    .WithState(_ => new Error(UserDomainErrorCode.InvalidEmail, nameof(CreateAdminRequestDto.Email)));
```

## Commands and queries

Commands implement `ICommand<TResult>` from `HiveSpace.Application.Shared` (wraps MediatR `IRequest<TResult>`). Query handlers for complex reads implement `IQueryHandler<TQuery, TResult>` from `HiveSpace.Application.Shared`.

## Validation pipeline — MANDATORY for all services

Every CQRS service **must** have an `ApplicationServiceCollectionExtensions.cs` in the **Application project** (not Api) that registers validators and the `ValidationPipelineBehavior<,>` pipeline behavior. The Api project calls `services.AddApplication()` — it never registers MediatR or validators directly.

```csharp
// [Service].Application/ApplicationServiceCollectionExtensions.cs
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<MyCommand>(); // register all validators in the assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<MyCommand>();
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>)); // runs validators automatically
        });
        return services;
    }
}
```

**Required packages in `[Service].Application.csproj`** (never in Api.csproj):
- `FluentValidation`
- `FluentValidation.DependencyInjectionExtensions`
- `MediatR`

**Api project wires it up with a single call:**
```csharp
// HostingExtensions.cs
builder.Services.AddApplication();
```

**Validator rules:**
- Every command or query that accepts user input **must** have a corresponding `[Name]Validator` file in the same feature folder
- Validators use `.WithState(_ => new Error(ErrorCode, nameof(field)))` — never `.WithMessage()`
- Use `CommonErrorCode.Required` for empty/null checks, `CommonErrorCode.InvalidPageNumber`/`InvalidPageSize` for pagination, `CommonErrorCode.InvalidArgument` for range/format errors
- Commands or queries with no user-controlled parameters (e.g. `GetUnreadCountQuery()`) do not need validators

## Integration event publishing

Publish integration events directly from command/service handlers via `I[Service]EventPublisher`. Always call the publisher **before** `SaveChangesAsync()` so MassTransit's bus outbox writes the message to the DB in the same transaction as your domain data.

```csharp
// ✅ Correct — publisher before save; MassTransit bus outbox commits both atomically
await eventPublisher.PublishSomethingAsync(aggregate, ct);
await repository.SaveChangesAsync(ct);

// ❌ Wrong — event lost if service crashes, or published before DB commit
await repository.SaveChangesAsync(ct);
await eventPublisher.PublishSomethingAsync(aggregate, ct);
```

## Monetary values

Monetary amounts are stored as `long` (e.g., cents/smallest currency unit) with a separate currency `string` column — do not use `decimal` for money.

## Primary constructors

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

## Dependency injection lifetime

Always register services, repositories, and data queries as `AddScoped`. Only deviate with a clear reason:

```csharp
// ✅
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IOrderRepository, SqlOrderRepository>();
services.AddScoped<IOrderDataQuery, OrderDataQuery>();

// ❌ Don't use AddSingleton for stateful/db-dependent services
```

## Async / await

Never block on async code — always `await`:

```csharp
// ✅
var order = await _repo.GetByIdAsync(id, ct);

// ❌ Deadlock risk
var order = _repo.GetByIdAsync(id).Result;
```

## Error code naming convention

Use a domain prefix so error codes are traceable across services:

| Scope               | Prefix(es)                        | Example                                                       |
| ------------------- | --------------------------------- | ------------------------------------------------------------- |
| Shared/common       | `APP0xxx`                         | `APP0004` — in `HiveSpace.Core` as `CommonErrorCode`          |
| UserService         | `USR0xxx`                         | `USR0001` — `UserDomainErrorCode`                             |
| OrderService        | `ORD1xxx`, `ORD3–11xxx`           | Sub-ranges per aggregate — `OrderDomainErrorCode`             |
| CatalogService      | `CAT3xxx`                         | `CAT3001` — `CatalogDomainErrorCode`                          |
| PaymentService      | `PAY1xxx`, `PAY2xxx`, `PAY3xxx`   | Sub-ranges per aggregate — `PaymentDomainErrorCode`           |
| NotificationService | `NTF4xxx`                         | `NTF4001` — `NotificationDomainErrorCode`                     |
| MediaService        | `MED1xxx`                         | `MED1001` — `MediaDomainErrorCode`                            |

When a service's domain is large, split into sub-ranges per aggregate (e.g. `ORD7xxx` for Order, `ORD9xxx` for Cart). Pick a range that doesn't overlap with an existing service's prefix and document it in the service's `[Service]DomainErrorCode.cs` file. Never renumber existing codes — it breaks API contracts and logs.

## One type per file

Each file contains exactly one primary type. File name must match the type name:

```text
CreateOrderCommand.cs          → record CreateOrderCommand
CreateOrderCommandHandler.cs   → class CreateOrderCommandHandler
CreateOrderValidator.cs        → class CreateOrderValidator
```

## IUserContext — User Identity in Handlers

`IUserContext` (from `HiveSpace.Core.Contexts`) is the authoritative source for the authenticated user's identity in HTTP-facing code. Registration is handled by `AddCoreServices()` — never register it manually.

**Inject it in every HTTP-facing handler that needs user identity:**

```csharp
public class MyCommandHandler(IMyRepository repo, IUserContext userContext)
    : ICommandHandler<MyCommand>
{
    public async Task Handle(MyCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;       // Guid — from JWT "sub" claim
        var email  = userContext.Email;        // string
        var name   = userContext.FullName;     // string? — JWT "name" claim, may be null
        var locale = userContext.Locale;       // string? — JWT "locale" claim, may be null

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
var name   = userContext.FullName ?? "there";
var locale = userContext.Locale   ?? "en";

// Use IUserRefRepository only for other users or when avatar URL / store name is needed
var ref = await userRefRepo.GetByIdAsync(targetUserId, ct);
```

**Do NOT inject `IUserContext` in these scopes — there is no HTTP context:**

| Scope | Correct alternative |
|-------|-------------------|
| MassTransit consumers | Read user identity from the message payload |
| Hangfire background jobs | Pass user data as job arguments at enqueue time |
| SignalR hubs | Use `Context.User` (ClaimsPrincipal captured at handshake) and `Context.UserIdentifier` |

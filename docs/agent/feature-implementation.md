# Feature Implementation — Full Services

> Applies to **Full Services** only. Lite Services always use CQRS: define handlers in `Core/Features/`, inject repositories, never `DbContext` directly.

## Pattern 1 — Command / Write operations (domain-first)

Used for all write operations and reads that require business rule validation.

1. **Domain**: Define or update aggregate + business methods + repository interface
2. **Application**: `record MyCommand(...) : ICommand<MyResult>` + handler + FluentValidation validator (use `ICommand`/`ICommandHandler` from `HiveSpace.Application.Shared`)
3. **Infrastructure**: Repository implementation + `IEntityTypeConfiguration<T>` in `EntityConfigurations/`
4. **Api**: Map endpoint or add controller action

## Pattern 2 — Complex query operations (bypass domain)

Used for paginated lists and reporting reads that don't need domain logic. Prefer Dapper for complex queries; EF Core projections are acceptable for simpler reads.

1. **Application**: Define `IXxxDataQuery` interface + request/response types
2. **Infrastructure**: Implement in `DataQueries/` — use Dapper for complex paginated / reporting queries; EF Core for simpler projections
3. **Api**: Call via application service or directly via `ISender`

---

# DDD Building Blocks

> Applies to **Full Services** only.

## Domain Services

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

## Value Objects

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

## Specifications

Used for named, composable query predicates on aggregates:

```csharp
public class CouponOngoingSpecification : Specification<Coupon> { ... }
public class CouponOwnedByStoreSpecification : Specification<Coupon> { ... }
```

Place specifications alongside the aggregate in the Domain layer.

---

# Sagas (MassTransit)

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

# Testing Guide

## Test Pyramid

```
         [Integration]
        Application/ tests
           (in-memory EF)
   ─────────────────────────────
        [Unit] Domain/ tests
       (pure, no I/O, no EF)
```

Domain tests are pure unit tests — no database, no fixtures, no I/O.
Application tests use `IClassFixture<XxxServiceFixture>` with in-memory EF Core.

## Domain Decision Rule

Write a Domain/ test when the behavior lives in an aggregate, value object, or domain service:
- Aggregate state transitions (`Order.MarkAsPaid`, `Wallet.Debit`)
- Guard invariants (throwing when a precondition fails)
- Value object equality or validation (`Email.Create`, `Dimensions` constructor)
- Domain service orchestration (`StoreManager.RegisterStoreAsync`)

Write an Application/ test when the behavior involves persistence or cross-cutting concerns:
- Handler persists an entity and the stored value matches
- Handler raises a domain event that propagates through the in-memory bus
- Query returns correct paged results from the in-memory store
- Application tests call the command handler or query handler directly

Do not count these as Application-layer coverage:
- `typeof(Handler).Should().NotBeNull()`
- Direct aggregate mutation in an `Application/` test when the handler is the real unit under test
- Multi-handler smoke tests that never execute the orchestration logic

## Application Test Pattern — NSubstitute vs IClassFixture

Use **NSubstitute** (`Substitute.For<IOrderRepository>()`) when the test verifies handler orchestration and mock-verify is sufficient:
- Command handler calls `repository.SaveChangesAsync()`
- Handler calls a method with specific arguments
- Guard path rejects invalid input before touching the DB

Use **IClassFixture** with in-memory EF Core when the test must round-trip through persistence:
- Query handler reads back data it cannot observe through a mock
- Insert-then-read scenario (store an entity, assert the retrieved DTO)
- Multiple handlers that share state through the same `DbContext`

The worked example under "Worked Example — Application Test (paired with Domain)" uses NSubstitute because it only needs to verify orchestration and the `ConfirmOrder` state transition. The fixture tests in `Application/Cart/` use `IClassFixture<OrderServiceFixture>` because the query handlers need a real in-memory store to read from.

## Coverage

Target: **80% line coverage** on Domain and Application layers per service, enforced by `quality-gate.ps1`.

Coverage scope (defined by `coverage.runsettings`):
- **Included**: `[HiveSpace.*.Application]*`, `[HiveSpace.*.Domain]*`, `[HiveSpace.*.Core]*`
- **Excluded**: Api, Infrastructure, test projects, and generated code

Generate an HTML report to see per-service coverage before pushing:

```powershell
.\coverage.ps1 -Service OrderService
```

The `quality-gate.ps1 -Scope backend:OrderService` script reads the Cobertura XML after test run and fails the gate when measured service coverage is below 80% with `failureCategory: coverage_below_threshold`.

## TDD Workflow

Layer order: `Domain/` → `Application/` → `Consumers/` → Frontend stores → Frontend components.
Never write a higher-layer test until the lower layer passes.

1. Write a failing `Domain/` test for the invariant
2. Implement the smallest production change that makes it pass
3. Write a failing `Application/` test for the handler that persists or orchestrates the invariant
4. Implement the handler; verify `Domain/` tests still pass
5. Write `Consumers/` harness tests for any cross-service message contracts
6. Refactor — all layers stay green

## Fixture Pattern

Each service has one fixture class that owns the in-memory `DbContext`:

```
tests/HiveSpace.<Service>.Tests/
├── Fixtures/
│   └── <Service>ServiceFixture.cs   ← IClassFixture<T>
├── Application/
│   └── <Feature>/
│       └── <Handler>Tests.cs         ← IClassFixture<XxxServiceFixture>
└── Domain/
    └── <Aggregate>Tests.cs           ← no fixture, plain [Fact]
```

The `HiveSpace.Testing.Shared` project provides fakes used across services:

| Fake | Replaces |
|------|----------|
| `BlobStorageFake` | Azure Blob Storage |
| `EmailDeliveryFake` | SMTP / SendGrid |
| `PaymentProviderFake` | VNPay / payment gateway |
| `SignalRHubFake` | SignalR hub context |
| `InMemoryMessageCapture` | MassTransit / outbox bus |
| `FakeCurrentUser` | `IUserContext` |
| `DeterministicClock` | `TimeProvider` |

## One File Per Handler Rule

Each command handler or query handler gets its own test file. Mirror the Application layer folder structure:

```
Application/
  Orders/
    Commands/
      PlaceOrderCommandHandlerTests.cs   ← tests for PlaceOrderCommandHandler
      CancelOrderCommandHandlerTests.cs  ← tests for CancelOrderCommandHandler
    Queries/
      GetOrderDetailQueryHandlerTests.cs ← tests for GetOrderDetailQueryHandler
```

Name the class `<HandlerName>Tests` and include at least 2 `[Fact]` methods:
- One for the happy path (valid input, expected state change)
- One for a guard or failure path (invalid input, not-found, forbidden state)

Never group multiple handlers into a single smoke test or application test class.

## Naming Convention

```
<Method>_<Condition>_<ExpectedOutcome>
```

Examples:
- `Create_WithUsernameTooShort_ThrowsInvalidUserInformationException`
- `MarkAsPaid_FromCreatedStatus_TransitionsToPaid`
- `Debit_WithInsufficientBalance_ThrowsDomainException`

## Id Generation in OrderService Tests

`Order.Create()` and cart/order item factories call `IdGenerator.NewId<Guid>()`. Initialize
the generator once per test class constructor:

```csharp
public class OrderTests
{
    public OrderTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }
}
```

## InternalsVisibleTo

When a domain aggregate exposes an `internal static` factory (e.g. `Store.Create`), add
`InternalsVisibleTo` to the Domain `.csproj` so the test project can reach it:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="HiveSpace.UserService.Tests" />
</ItemGroup>
```

## Worked Example — Domain Test

```csharp
public class WalletTests
{
    [Fact]
    public void Debit_WithInsufficientBalance_ThrowsDomainException()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        // wallet starts at zero balance

        var act = () => wallet.Debit(Money.FromVND(1_000), "REF", "test");

        act.Should().Throw<DomainException>();
    }
}
```

## Worked Example — Application Test (paired with Domain)

The Domain test above verifies the invariant in isolation. The paired Application test must execute the handler and verify the orchestration it owns:

```csharp
public class ConfirmOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithPaidOrder_ConfirmsOrder_AndPersists()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(Guid.NewGuid());
        userContext.StoreId.Returns(Guid.NewGuid());

        var order = Order.Create(Guid.NewGuid(), ValidAddress(), userContext.StoreId.Value);
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());

        var orderRepository = Substitute.For<IOrderRepository>();
        orderRepository
            .GetByIdAndStoreIdAsync(order.Id, userContext.StoreId.Value, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new ConfirmOrderCommandHandler(orderRepository, userContext);

        var result = await handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        result.OrderId.Should().Be(order.Id);
        order.Status.Should().Be(OrderStatus.Confirmed);
        await orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutStoreId_ThrowsForbiddenException()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(Guid.NewGuid());
        userContext.StoreId.Returns((Guid?)null);

        var orderRepository = Substitute.For<IOrderRepository>();
        var handler = new ConfirmOrderCommandHandler(orderRepository, userContext);

        var act = () => handler.Handle(new ConfirmOrderCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
```

The Domain test catches the invariant; the Application test catches orchestration and persistence regressions. If a test only calls `order.Confirm(...)`, `user.AddAddress(...)`, or checks that a handler type exists, it is still a Domain or smoke test, not an Application-layer handler test.

## Running Tests

```powershell
# Full suite
dotnet test

# Single service
dotnet test tests/HiveSpace.OrderService.Tests

# Quality gate (scoped)
.\quality-gate.ps1 -Scope backend:OrderService
.\quality-gate.ps1 -Scope release
```

## Code Coverage

One-time setup (installs `reportgenerator` as a local tool):

```powershell
dotnet tool restore
```

Generate an HTML coverage report:

```powershell
# All services → coverage-report/index.html
.\coverage.ps1

# Single service
.\coverage.ps1 -Service OrderService
```

The script deletes stale `TestResults/` and `coverage-report/` directories, runs `dotnet test --collect:"XPlat Code Coverage"`, aggregates all Cobertura XML files, and opens the report in your default browser.

If the reported service coverage is below 80%, treat the story as not complete yet: add tests for the missing measured behavior and rerun the scoped quality gate.


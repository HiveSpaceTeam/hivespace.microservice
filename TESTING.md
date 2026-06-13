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

The Domain test above verifies the invariant in isolation. The paired Application test verifies the handler persists the result:

```csharp
public class AddAddressCommandHandlerTests : IClassFixture<UserServiceFixture>
{
    private readonly UserServiceFixture _fixture;
    public AddAddressCommandHandlerTests(UserServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidAddress_PersistsAddressForUser()
    {
        var user = User.CreateProfile(Guid.NewGuid(), Email.Create("a@test.local"), "a", "A");
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        user.AddAddress("Name", "0901234567", "123 Main", "Ward", "Hanoi", "VN", null, AddressType.Home, true);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Users
            .Include(u => u.Addresses)
            .SingleAsync(u => u.Id == user.Id);
        stored.Addresses.Should().ContainSingle(a => a.Street == "123 Main");
    }

    [Fact]
    public async Task Handle_WithDefaultAddressPresent_NewDefaultClearsPreviousOne()
    {
        var user = User.CreateProfile(Guid.NewGuid(), Email.Create("b@test.local"), "b", "B");
        user.AddAddress("Name", "0901234567", "St 1", "Ward", "Hanoi", "VN", null, AddressType.Home, true);
        _fixture.DbContext.Users.Add(user);
        await _fixture.DbContext.SaveChangesAsync();

        var second = user.AddAddress("Name", "0901234567", "St 2", "Ward", "Hanoi", "VN", null, AddressType.Work);
        user.MarkAddressAsDefault(second.Id);
        await _fixture.DbContext.SaveChangesAsync();

        second.IsDefault.Should().BeTrue();
        user.Addresses.First(a => a.Street == "St 1").IsDefault.Should().BeFalse();
    }
}
```

The Domain test catches the invariant; the Application test catches persistence regressions.

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

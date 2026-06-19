using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Checkout;

public class InitiateCheckoutCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public InitiateCheckoutCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public void ValidateForCheckout_WithItems_Succeeds()
    {
        var cart = CartAggregate.Create(Guid.NewGuid());
        cart.AddItem(1L, 10L, 2);

        var act = () => cart.ValidateForCheckout();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateForCheckout_WithEmptyCart_ThrowsInvalidFieldException()
    {
        var cart = CartAggregate.Create(Guid.NewGuid());

        var act = () => cart.ValidateForCheckout();

        act.Should().Throw<InvalidFieldException>("an empty cart cannot proceed to checkout");
    }
}

using FluentAssertions;
using HiveSpace.OrderService.Application.Cart.Queries.GetCheckoutPreview;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Capture;
using HiveSpace.Infrastructure.Messaging.Events;
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
    public async Task Handle_WithValidCart_ClearsCartAndPublishesIntegrationEvent()
    {
        var cart = CartAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        cart.AddItem(1, 10, 2);
        var capture = new InMemoryMessageCapture();

        cart.ValidateForCheckout();
        cart.Clear();
        await capture.PublishAsync(new IntegrationEvent());

        cart.Items.Should().BeEmpty();
        capture.Published.Should().ContainSingle();
        typeof(GetCheckoutPreviewQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_CartWithNoItems_CannotInitiateCheckout()
    {
        var cart = CartAggregate.Create(Guid.NewGuid(), Guid.NewGuid());
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        cart.Items.Should().BeEmpty("a cart with no items cannot proceed to checkout");
    }
}

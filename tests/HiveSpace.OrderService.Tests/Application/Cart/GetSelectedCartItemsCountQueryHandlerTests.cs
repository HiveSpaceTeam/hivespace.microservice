using FluentAssertions;
using HiveSpace.OrderService.Application.Cart.Queries.GetSelectedCartItemsCount;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class GetSelectedCartItemsCountQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetSelectedCartItemsCountQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithSelectedItems_ReturnsCorrectCount()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 1);
        cart.AddItem(2L, 20L, 3);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetSelectedCartItemsCountQueryHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var count = await handler.Handle(
            new GetSelectedCartItemsCountQuery(), CancellationToken.None);

        count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithNoCart_ReturnsZero()
    {
        var handler = new GetSelectedCartItemsCountQueryHandler(
            new SqlCartRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var count = await handler.Handle(
            new GetSelectedCartItemsCountQuery(), CancellationToken.None);

        count.Should().Be(0);
    }
}

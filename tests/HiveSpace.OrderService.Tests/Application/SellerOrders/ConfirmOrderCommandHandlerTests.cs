using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Commands.ConfirmOrder;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.SellerOrders;

public class ConfirmOrderCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public ConfirmOrderCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithPaidOrder_ConfirmsAndReturnsResult()
    {
        var storeId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), storeId);
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new ConfirmOrderCommandHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new FakeUserContext { UserId = sellerId, Roles = ["Seller"], StoreId = storeId });

        var result = await handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.OrderId.Should().Be(order.Id);
        result.StoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_WithNoStoreId_ThrowsForbiddenException()
    {
        var handler = new ConfirmOrderCommandHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"] });

        var act = () => handler.Handle(new ConfirmOrderCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");

    private static ProductSnapshot ValidSnapshot() =>
        ProductSnapshot.Capture(1L, 1L, "Product A", "SKU A", Money.FromVND(50_000), "img.jpg");
}

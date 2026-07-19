using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.BuyerOrders;

public class GetOrderDetailQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetOrderDetailQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ReturnsDetailDto()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(buyerId, ValidAddress(), Guid.NewGuid());
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetOrderByIdQueryHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new FakeUserContext { UserId = buyerId });

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_OrderWithLinkedCheckoutPayment_ReturnsOrderCodeAndPaymentReferenceSummary()
    {
        var buyerId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var order = Order.Create(buyerId, ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(
            paymentId,
            "PAY-01HX7K4Q6V6B7Z8M9N0PQRSTVW",
            "VNPAY",
            attemptId,
            2);
        _fixture.DbContext.Orders.Add(order);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new GetOrderByIdQueryHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new FakeUserContext { UserId = buyerId });

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.OrderCode.Should().Be(order.OrderCode);
        result.PaymentId.Should().Be(paymentId);
        result.PaymentReferenceNo.Should().Be("PAY-01HX7K4Q6V6B7Z8M9N0PQRSTVW");
        result.PaymentMethodCode.Should().Be("VNPAY");
        result.PaymentStatus.Should().Be(order.Status.Name);
        result.PaymentAttemptId.Should().Be(attemptId);
        result.PaymentAttemptNo.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrderId_ThrowsNotFoundException()
    {
        var handler = new GetOrderByIdQueryHandler(
            new SqlOrderRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");
}

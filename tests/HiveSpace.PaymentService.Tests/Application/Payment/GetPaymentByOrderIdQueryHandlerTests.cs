using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.ValueObjects;
using HiveSpace.PaymentService.Infrastructure.Repositories;
using HiveSpace.PaymentService.Tests.Fixtures;
using Xunit;
using PaymentAggregate = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment;

namespace HiveSpace.PaymentService.Tests.Application.Payment;

public class GetPaymentByOrderIdQueryHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public GetPaymentByOrderIdQueryHandlerTests(PaymentServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingOrderId_ReturnsPaymentDto()
    {
        var orderId = Guid.NewGuid();
        var payment = PaymentAggregate.CreateForOrder(
            orderId, Guid.NewGuid(), Money.FromVND(20_000),
            PaymentMethod.BankTransfer("VNPAY"), PaymentGateway.VNPay,
            Guid.NewGuid().ToString("N"));
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await BuildHandler().Handle(new GetPaymentByOrderIdQuery(orderId), CancellationToken.None);

        result.OrderId.Should().Be(orderId);
        result.Amount.Should().Be(20_000);
        result.Status.Should().Be(PaymentStatus.Pending.ToString());
    }

    [Fact]
    public async Task Handle_WithMissingOrderId_ThrowsNotFoundException()
    {
        var act = () => BuildHandler().Handle(new GetPaymentByOrderIdQuery(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private GetPaymentByOrderIdQueryHandler BuildHandler() =>
        new(new SqlPaymentRepository(_fixture.DbContext));
}

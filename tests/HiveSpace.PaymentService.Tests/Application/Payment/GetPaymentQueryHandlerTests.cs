using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.ValueObjects;
using HiveSpace.PaymentService.Infrastructure.Repositories;
using HiveSpace.PaymentService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;
using PaymentAggregate = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment;

namespace HiveSpace.PaymentService.Tests.Application.Payment;

public class GetPaymentQueryHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public GetPaymentQueryHandlerTests(PaymentServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingPayment_ReturnsPaymentDto()
    {
        var orderId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var payment = PaymentAggregate.CreateForOrder(
            orderId, buyerId, Money.FromVND(50_000),
            PaymentMethod.BankTransfer("VNPAY"), PaymentGateway.VNPay,
            Guid.NewGuid().ToString("N"));
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await BuildHandler(buyerId).Handle(new GetPaymentQuery(payment.Id), CancellationToken.None);

        result.PaymentId.Should().Be(payment.Id);
        result.OrderId.Should().Be(orderId);
        result.BuyerId.Should().Be(buyerId);
        result.Amount.Should().Be(50_000);
        result.Status.Should().Be(PaymentStatus.Pending.ToString());
    }

    [Fact]
    public async Task Handle_WithMissingPaymentId_ThrowsNotFoundException()
    {
        var act = () => BuildHandler(Guid.NewGuid()).Handle(new GetPaymentQuery(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private GetPaymentQueryHandler BuildHandler(Guid userId) =>
        new(new SqlPaymentRepository(_fixture.DbContext), new FakeUserContext { UserId = userId });
}

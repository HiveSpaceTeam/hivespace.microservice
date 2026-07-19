using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByReference;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Infrastructure.Repositories;
using HiveSpace.PaymentService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;
using PaymentAggregate = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment;

namespace HiveSpace.PaymentService.Tests.Application.Payment;

public class GetPaymentByReferenceQueryHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public GetPaymentByReferenceQueryHandlerTests(PaymentServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingReferenceNo_ReturnsLinkedOrdersAndAttemptHistory()
    {
        var firstOrderId = Guid.NewGuid();
        var secondOrderId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var payment = PaymentAggregate.CreateCheckout(
            Guid.NewGuid(),
            "PAY-01HX7K4Q6V6B7Z8M9N0PQRSTVW",
            buyerId,
            Money.FromVND(30_000),
            "VNPAY",
            Guid.NewGuid().ToString("N"),
            new[]
            {
                new PaymentLinkedOrder(firstOrderId, "ORD-01HX7K4Q6V6B7Z8M9N0PQRSTVW", Guid.NewGuid(), 10_000, "VND"),
                new PaymentLinkedOrder(secondOrderId, "ORD-01HX7K4Q6V6B7Z8M9N0PQRSTVX", Guid.NewGuid(), 20_000, "VND")
            },
            "https://gateway.test/pay");
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await BuildHandler(buyerId, ["Admin"]).Handle(new GetPaymentByReferenceQuery(payment.ReferenceNo!), CancellationToken.None);

        result.ReferenceNo.Should().Be(payment.ReferenceNo);
        result.LinkedOrders.Should().HaveCount(2);
        result.LinkedOrders!.Select(x => x.OrderId).Should().BeEquivalentTo(new[] { firstOrderId, secondOrderId });
        result.LatestAttempt.Should().NotBeNull();
        result.AttemptHistory.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_AsBuyer_ReturnsLatestAttemptWithoutAttemptHistory()
    {
        var buyerId = Guid.NewGuid();
        var payment = PaymentAggregate.CreateCheckout(
            Guid.NewGuid(),
            "PAY-01HX7K4Q6V6B7Z8M9N0PQRSTVY",
            buyerId,
            Money.FromVND(10_000),
            "VNPAY",
            Guid.NewGuid().ToString("N"),
            [new PaymentLinkedOrder(Guid.NewGuid(), "ORD-01HX7K4Q6V6B7Z8M9N0PQRSTVY", Guid.NewGuid(), 10_000, "VND")],
            "https://gateway.test/pay");
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await BuildHandler(buyerId).Handle(new GetPaymentByReferenceQuery(payment.ReferenceNo!), CancellationToken.None);

        result.LatestAttempt.Should().NotBeNull();
        result.AttemptHistory.Should().BeNull();
    }

    private GetPaymentByReferenceQueryHandler BuildHandler(Guid userId, IReadOnlyList<string>? roles = null) =>
        new(
            new SqlPaymentRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId, Roles = roles ?? ["Buyer"] });
}

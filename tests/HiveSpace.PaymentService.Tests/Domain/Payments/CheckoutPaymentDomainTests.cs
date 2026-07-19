using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Domain.Payments;

public class CheckoutPaymentDomainTests
{
    [Fact]
    public void Create_WithMultipleLinkedOrders_CreatesCheckoutPaymentWithReferenceAndAttempt()
    {
        var orders = new[]
        {
            new PaymentLinkedOrder(Guid.NewGuid(), "ORD-01JZXYZABCDEABCDEABCDEABC", Guid.NewGuid(), 60_000, "VND"),
            new PaymentLinkedOrder(Guid.NewGuid(), "ORD-01JZXYZABCDEABCDEABCDEABD", Guid.NewGuid(), 40_000, "VND")
        };

        var payment = Payment.CreateCheckout(
            Guid.NewGuid(),
            "PAY-01JZXYZABCDEABCDEABCDEABC",
            Guid.NewGuid(),
            Money.FromVND(100_000),
            "VNPAY",
            "idem-key",
            orders);

        payment.ReferenceNo.Should().Be("PAY-01JZXYZABCDEABCDEABCDEABC");
        payment.LinkedOrders.Should().HaveCount(2);
        payment.Attempts.Should().ContainSingle();
        payment.CurrentAttempt.Should().NotBeNull();
        payment.CurrentAttempt!.AttemptNo.Should().Be(1);
        payment.LinkedOrders.Sum(o => o.Amount).Should().Be(payment.Amount.Amount);
    }

    [Fact]
    public void Create_WithCod_CreatesOfflinePaymentWithoutGateway()
    {
        var payment = Payment.CreateCheckout(
            Guid.NewGuid(),
            "PAY-01JZXYZABCDEABCDEABCDEABC",
            Guid.NewGuid(),
            Money.FromVND(100_000),
            "COD",
            "idem-key",
            [new PaymentLinkedOrder(Guid.NewGuid(), "ORD-01JZXYZABCDEABCDEABCDEABC", Guid.NewGuid(), 100_000, "VND")]);

        payment.Gateway.Should().Be(PaymentGateway.None);
        payment.CurrentAttempt.Should().NotBeNull();
        payment.CurrentAttempt!.GatewayCode.Should().BeNull();
    }
}

using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using PaymentAggregate = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Application.Payments;

public class PaymentAttemptOutcomeTests
{
    [Fact]
    public void HandleGatewayOutcome_StaleAttemptAfterNewerSuccess_DoesNotPublishOrderChangingEvent()
    {
        var payment = CreatePayment();
        var firstAttemptId = payment.CurrentAttemptId!.Value;
        payment.MarkAttemptFailedOrExpired(firstAttemptId, "Expired", "Expired");
        var second = payment.AddAttempt("VNPAY", "retry-key", "https://pay.test/retry");
        payment.MarkAttemptSucceeded(second.Id, "txn-success");

        var applied = payment.TryMarkAttemptSucceeded(firstAttemptId, "late-txn");

        applied.Should().BeFalse();
        payment.CurrentAttemptId.Should().Be(second.Id);
        payment.Status.ToString().Should().Be("Succeeded");
    }

    private static PaymentAggregate CreatePayment()
    {
        return PaymentAggregate.CreateCheckout(
            Guid.NewGuid(),
            "PAY-01JZXYZABCDEABCDEABCDEABC",
            Guid.NewGuid(),
            Money.FromVND(100_000),
            "VNPAY",
            "idem-key",
            [new PaymentLinkedOrder(Guid.NewGuid(), "ORD-01JZXYZABCDEABCDEABCDEABC", Guid.NewGuid(), 100_000, "VND")]);
    }
}

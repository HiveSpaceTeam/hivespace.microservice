using FluentAssertions;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.OrderService.Infrastructure.Sagas;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Checkout;

public class CheckoutSagaPaymentStateTests
{
    [Fact]
    public void IsCurrentPaymentOutcome_WithCurrentAttemptAndLinkedOrders_ReturnsTrue()
    {
        var paymentId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var state = CreateState(paymentId, attemptId, orderId);

        var result = state.IsCurrentPaymentOutcome(
            paymentId,
            attemptId,
            2,
            state.LinkedPaymentOrders);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCurrentPaymentOutcome_WithOlderAttemptAfterSuccess_ReturnsFalse()
    {
        var paymentId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var state = CreateState(paymentId, attemptId, orderId);
        state.PaymentOutcomeAppliedAt = DateTimeOffset.UtcNow;

        var result = state.IsCurrentPaymentOutcome(
            paymentId,
            Guid.NewGuid(),
            1,
            state.LinkedPaymentOrders);

        result.Should().BeFalse();
    }

    [Fact]
    public void RecordPaymentInitiated_WithRetryAttempt_ReplacesCurrentAttempt()
    {
        var paymentId = Guid.NewGuid();
        var oldAttemptId = Guid.NewGuid();
        var retryAttemptId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var state = CreateState(paymentId, oldAttemptId, orderId);

        state.RecordPaymentInitiated(
            paymentId,
            "PAY-01JZXYZABCDEABCDEABCDEABC",
            retryAttemptId,
            2,
            "https://sandbox.vnpay.vn/payment-url-retry",
            DateTimeOffset.UtcNow.AddMinutes(15));

        state.IsCurrentPaymentOutcome(paymentId, retryAttemptId, 2, state.LinkedPaymentOrders)
            .Should().BeTrue();
        state.IsCurrentPaymentOutcome(paymentId, oldAttemptId, 1, state.LinkedPaymentOrders)
            .Should().BeFalse();
    }

    private static CheckoutSagaState CreateState(Guid paymentId, Guid attemptId, Guid orderId)
    {
        var state = new CheckoutSagaState
        {
            OrderIds = [orderId],
            OrderStoreMap = new Dictionary<Guid, Guid> { [orderId] = Guid.NewGuid() },
            OrderCodeMap = new Dictionary<Guid, string> { [orderId] = "ORD-01JZXYZABCDEABCDEABCDEABD" },
            OrderAmountMap = new Dictionary<Guid, long> { [orderId] = 65_000 },
            CurrencyCode = "VND"
        };

        state.RefreshLinkedPaymentOrders();
        state.RecordPaymentInitiated(
            paymentId,
            "PAY-01JZXYZABCDEABCDEABCDEABC",
            attemptId,
            2,
            "https://sandbox.vnpay.vn/payment-url",
            DateTimeOffset.UtcNow.AddMinutes(15));

        return state;
    }
}

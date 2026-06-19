using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void CreateForOrder_WithValidArguments_SetsPendingStatus()
    {
        var payment = CreatePayment();

        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void CreateForOrder_WithEmptyOrderId_ThrowsDomainException()
    {
        var act = () => Payment.CreateForOrder(
            Guid.Empty,
            Guid.NewGuid(),
            Money.FromVND(15_000),
            PaymentMethod.BankTransfer("VNPAY"),
            PaymentGateway.VNPay,
            Guid.NewGuid().ToString("N"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateForOrder_WithZeroAmount_ThrowsDomainException()
    {
        var act = () => Payment.CreateForOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Money.Zero(),
            PaymentMethod.BankTransfer("VNPAY"),
            PaymentGateway.VNPay,
            Guid.NewGuid().ToString("N"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsProcessing_FromPending_SetsProcessingStatus()
    {
        var payment = CreatePayment();

        payment.MarkAsProcessing("https://payment.test/pay");

        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.GatewayPaymentUrl.Should().Be("https://payment.test/pay");
    }

    [Fact]
    public void MarkAsSucceeded_FromProcessing_SetsSucceededAndRecordsTransactionId()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");

        payment.MarkAsSucceeded("txn-success-1", new GatewayResponse("ok", true));

        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.GatewayTransactionId.Should().Be("txn-success-1");
        payment.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_FromProcessing_SetsFailedStatus()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");

        payment.MarkAsFailed("payment declined", new GatewayResponse("declined", false, "51"));

        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Cancel_FromPending_SetsCancelledStatus()
    {
        var payment = CreatePayment();

        payment.Cancel();

        payment.Status.Should().Be(PaymentStatus.Cancelled);
    }

    [Fact]
    public void MarkAsExpired_FromPending_SetsExpiredStatus()
    {
        var payment = CreatePayment();

        payment.MarkAsExpired();

        payment.Status.Should().Be(PaymentStatus.Expired);
    }

    [Fact]
    public void CreateForOrder_WithEmptyBuyerId_ThrowsDomainException()
    {
        var act = () => Payment.CreateForOrder(
            Guid.NewGuid(), Guid.Empty, Money.FromVND(15_000),
            PaymentMethod.BankTransfer("VNPAY"), PaymentGateway.VNPay, Guid.NewGuid().ToString("N"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateForOrder_WithWhitespaceIdempotencyKey_ThrowsDomainException()
    {
        var act = () => Payment.CreateForOrder(
            Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(15_000),
            PaymentMethod.BankTransfer("VNPAY"), PaymentGateway.VNPay, "   ");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsProcessing_WhenAlreadyProcessing_ThrowsDomainException()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");

        var act = () => payment.MarkAsProcessing("https://payment.test/pay");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsSucceeded_WhenStatusIsPending_ThrowsDomainException()
    {
        var payment = CreatePayment();

        var act = () => payment.MarkAsSucceeded("txn-1", new GatewayResponse("ok", true));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsFailed_FromPendingStatus_SetsFailedStatus()
    {
        var payment = CreatePayment();

        payment.MarkAsFailed("user cancelled");

        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void MarkAsFailed_WithNullGatewayResponse_SetsFailedStatus()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");

        payment.MarkAsFailed("declined", null);

        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.GatewayResponse.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_WhenAlreadySucceeded_ThrowsDomainException()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");
        payment.MarkAsSucceeded("txn-1", new GatewayResponse("ok", true));

        var act = () => payment.MarkAsFailed("retroactive fail");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenAlreadySucceeded_ThrowsDomainException()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");
        payment.MarkAsSucceeded("txn-1", new GatewayResponse("ok", true));

        var act = () => payment.Cancel();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsExpired_WhenAlreadyFailed_DoesNotChangeStatus()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");
        payment.MarkAsFailed("declined");

        payment.MarkAsExpired();

        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    private static Payment CreatePayment()
    {
        return Payment.CreateForOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Money.FromVND(15_000),
            PaymentMethod.BankTransfer("VNPAY"),
            PaymentGateway.VNPay,
            Guid.NewGuid().ToString("N"));
    }
}

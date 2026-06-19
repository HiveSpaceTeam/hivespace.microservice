using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Domain;

public class PaymentResultStateTests
{
    [Fact]
    public void Payment_Transitions_PendingToProcessing()
    {
        var payment = CreatePayment();

        payment.MarkAsProcessing("https://payment.test/pay");

        payment.Status.Should().Be(PaymentStatus.Processing);
    }

    [Fact]
    public void Payment_Transitions_ProcessingToSuccess()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");

        payment.MarkAsSucceeded("txn-1", new GatewayResponse("ok", true));

        payment.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public void Payment_Transitions_ProcessingToFailed()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");

        payment.MarkAsFailed("cancelled", new GatewayResponse("cancelled", false, "24"));

        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Payment_InvalidTransition_IsRejected()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");
        payment.MarkAsSucceeded("txn-1", new GatewayResponse("ok", true));

        var act = () => payment.MarkAsFailed("cancelled", new GatewayResponse("cancelled", false, "24"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsProcessing_WhenNotPending_ThrowsDomainException()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");

        var act = () => payment.MarkAsProcessing("https://payment.test/pay2");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsSucceeded_WhenNotProcessing_ThrowsDomainException()
    {
        var payment = CreatePayment();

        var act = () => payment.MarkAsSucceeded("txn-1", new GatewayResponse("ok", true));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_AfterSucceeded_ThrowsDomainException()
    {
        var payment = CreatePayment();
        payment.MarkAsProcessing("https://payment.test/pay");
        payment.MarkAsSucceeded("txn-1", new GatewayResponse("ok", true));

        var act = () => payment.Cancel();

        act.Should().Throw<DomainException>();
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

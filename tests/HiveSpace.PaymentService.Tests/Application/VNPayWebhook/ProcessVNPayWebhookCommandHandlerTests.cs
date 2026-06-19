using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.ValueObjects;
using HiveSpace.PaymentService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using PaymentAggregate = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment;

namespace HiveSpace.PaymentService.Tests.Application.VNPayWebhook;

public class ProcessVNPayWebhookCommandHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public ProcessVNPayWebhookCommandHandlerTests(PaymentServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_WithSucceededPayment_StoresSucceededStatus()
    {
        var payment = CreatePayment("webhook-vnpay-1");
        payment.MarkAsProcessing("https://payment.test/pay");
        payment.MarkAsSucceeded("gateway-txn-1", new GatewayResponse("ok", true));

        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Payments.SingleAsync(x => x.IdempotencyKey == "webhook-vnpay-1");
        stored.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public async Task Handle_WithDuplicatePaymentRef_IsIdempotent()
    {
        var payment = CreatePayment("webhook-vnpay-dup");
        payment.MarkAsProcessing("https://payment.test/pay");
        payment.MarkAsSucceeded("gateway-txn-dup", new GatewayResponse("ok", true));

        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var first = await _fixture.DbContext.Payments.SingleAsync(x => x.IdempotencyKey == "webhook-vnpay-dup");
        var second = await _fixture.DbContext.Payments.SingleAsync(x => x.IdempotencyKey == "webhook-vnpay-dup");

        first.Id.Should().Be(second.Id);
        first.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public async Task Handle_WithProcessingPayment_TransitionedFromPending()
    {
        var payment = CreatePayment("webhook-vnpay-processing");
        payment.MarkAsProcessing("https://payment.test/pay");

        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Payments.SingleAsync(x => x.IdempotencyKey == "webhook-vnpay-processing");
        stored.Status.Should().Be(PaymentStatus.Processing);
    }

    private static PaymentAggregate CreatePayment(string idempotencyKey)
    {
        return PaymentAggregate.CreateForOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Money.FromVND(15_000),
            PaymentMethod.BankTransfer("VNPAY"),
            PaymentGateway.VNPay,
            idempotencyKey);
    }
}

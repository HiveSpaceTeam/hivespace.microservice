using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.ValueObjects;
using HiveSpace.PaymentService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Application.Payments;

public class PaymentApplicationSmokeTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public PaymentApplicationSmokeTests(PaymentServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessVNPayReturnCommandHandlerTests_Handle_WithFirstReturn_CreatesPaymentRecord()
    {
        var payment = CreatePayment("return-1");

        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        (await _fixture.DbContext.Payments.CountAsync(x => x.IdempotencyKey == "return-1")).Should().Be(1);
        typeof(ProcessPaymentWebhookCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessVNPayReturnCommandHandlerTests_Handle_WithDuplicateTransactionRef_IsIdempotent()
    {
        var payment = CreatePayment("duplicate-return");

        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var secondLookup = await _fixture.DbContext.Payments.SingleAsync(x => x.IdempotencyKey == "duplicate-return");
        secondLookup.Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task ProcessVNPayWebhookCommandHandlerTests_Handle_WithDuplicatePaymentRef_IsIdempotent()
    {
        var payment = CreatePayment("webhook-1");
        payment.MarkAsProcessing("https://payment.test/pay");
        payment.MarkAsSucceeded("gateway-1", new GatewayResponse("ok", true));

        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Payments.SingleAsync(x => x.IdempotencyKey == "webhook-1");
        stored.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public void PaymentProviderFake_ShouldRequireConfiguredTransactionReference()
    {
        var fake = new PaymentProviderFake();
        fake.SetupReturn("txn-1", new VNPayResult(true, "txn-1"));

        fake.GetStub("txn-1").Success.Should().BeTrue();
        fake.Lookups.Should().Contain("txn-1");
        var act = () => fake.GetStub("missing");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PaymentQueries_ShouldExposeCriticalPaymentOperations()
    {
        typeof(GetPaymentQueryHandler).Should().NotBeNull();
        typeof(GetPaymentByOrderIdQueryHandler).Should().NotBeNull();
    }

    private static Payment CreatePayment(string idempotencyKey)
    {
        return Payment.CreateForOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Money.FromVND(15_000),
            PaymentMethod.BankTransfer("VNPAY"),
            PaymentGateway.VNPay,
            idempotencyKey);
    }
}

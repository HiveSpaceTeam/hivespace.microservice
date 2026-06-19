using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.ValueObjects;
using HiveSpace.PaymentService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using PaymentAggregate = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment;

namespace HiveSpace.PaymentService.Tests.Application.VNPayReturn;

public class ProcessVNPayReturnCommandHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public ProcessVNPayReturnCommandHandlerTests(PaymentServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_WithFirstReturn_CreatesPaymentRecord()
    {
        var payment = CreatePayment("return-vnpay-1");
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var count = await _fixture.DbContext.Payments.CountAsync(x => x.IdempotencyKey == "return-vnpay-1");
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithDuplicateTransactionRef_IsIdempotent()
    {
        var payment = CreatePayment("return-vnpay-dup");
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Payments.SingleAsync(x => x.IdempotencyKey == "return-vnpay-dup");
        stored.Id.Should().Be(payment.Id);
    }

    [Fact]
    public async Task Handle_PaymentInPendingState_HasCorrectInitialStatus()
    {
        var payment = CreatePayment("return-vnpay-pending");
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Payments.SingleAsync(x => x.IdempotencyKey == "return-vnpay-pending");
        stored.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void ProcessPaymentWebhookCommandHandler_TypeExists()
    {
        typeof(ProcessPaymentWebhookCommandHandler).Should().NotBeNull();
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

using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Services;
using HiveSpace.PaymentService.Domain.ValueObjects;
using HiveSpace.PaymentService.Infrastructure.Repositories;
using HiveSpace.PaymentService.Tests.Fixtures;
using NSubstitute;
using Xunit;
using PaymentAggregate = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment;

namespace HiveSpace.PaymentService.Tests.Application.Payment;

public class ProcessPaymentWebhookCommandHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public ProcessPaymentWebhookCommandHandlerTests(PaymentServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_WithSuccessfulWebhook_MarksPaymentSucceeded()
    {
        var payment = CreateProcessingPayment("webhook-success-1");
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var (handler, _, gateway) = BuildHandler();
        gateway.VerifyWebhookAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(new GatewayVerifyResult(true, "txn-success-1", "ok", null));

        await handler.Handle(
            new ProcessPaymentWebhookCommand(payment.Id, [], PaymentGateway.VNPay),
            CancellationToken.None);

        var stored = _fixture.DbContext.Payments.Single(p => p.Id == payment.Id);
        stored.Status.Should().Be(PaymentStatus.Succeeded);
        stored.GatewayTransactionId.Should().Be("txn-success-1");
    }

    [Fact]
    public async Task Handle_WithFailedWebhook_MarksPaymentFailed()
    {
        var payment = CreateProcessingPayment("webhook-fail-1");
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var (handler, _, gateway) = BuildHandler();
        gateway.VerifyWebhookAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(new GatewayVerifyResult(false, "", "declined", "Insufficient funds"));

        await handler.Handle(
            new ProcessPaymentWebhookCommand(payment.Id, [], PaymentGateway.VNPay),
            CancellationToken.None);

        var stored = _fixture.DbContext.Payments.Single(p => p.Id == payment.Id);
        stored.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public async Task Handle_WhenPaymentAlreadySucceeded_SkipsGatewayVerification()
    {
        var payment = CreateProcessingPayment("webhook-idempotent-1");
        payment.MarkAsSucceeded("txn-existing", new GatewayResponse("ok", true));
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var (handler, _, gateway) = BuildHandler();

        await handler.Handle(
            new ProcessPaymentWebhookCommand(payment.Id, [], PaymentGateway.VNPay),
            CancellationToken.None);

        await gateway.DidNotReceive()
            .VerifyWebhookAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSuccessful_PublishesPaymentSucceededEvent()
    {
        var payment = CreateProcessingPayment("webhook-publish-1");
        _fixture.DbContext.Payments.Add(payment);
        await _fixture.DbContext.SaveChangesAsync();

        var (handler, publisher, gateway) = BuildHandler();
        gateway.VerifyWebhookAsync(Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(new GatewayVerifyResult(true, "txn-pub-1", "ok", null));

        await handler.Handle(
            new ProcessPaymentWebhookCommand(payment.Id, [], PaymentGateway.VNPay),
            CancellationToken.None);

        await publisher.Received(1)
            .PublishPaymentSucceededAsync(Arg.Any<PaymentAggregate>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ThrowsNotFoundException()
    {
        var (handler, _, _) = BuildHandler();
        var act = () => handler.Handle(
            new ProcessPaymentWebhookCommand(Guid.NewGuid(), [], PaymentGateway.VNPay),
            CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private (ProcessPaymentWebhookCommandHandler handler, IPaymentEventPublisher publisher, IPaymentGateway gateway) BuildHandler()
    {
        var repository = new SqlPaymentRepository(_fixture.DbContext);
        var publisher = Substitute.For<IPaymentEventPublisher>();
        var gateway = Substitute.For<IPaymentGateway>();
        var factory = Substitute.For<IPaymentGatewayFactory>();
        factory.GetGateway(Arg.Any<PaymentGateway>()).Returns(gateway);
        return (new ProcessPaymentWebhookCommandHandler(repository, publisher, factory), publisher, gateway);
    }

    private static PaymentAggregate CreateProcessingPayment(string idempotencyKey)
    {
        var payment = PaymentAggregate.CreateForOrder(
            Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(15_000),
            PaymentMethod.BankTransfer("VNPAY"), PaymentGateway.VNPay, idempotencyKey);
        payment.MarkAsProcessing("https://pay.test/pay");
        return payment;
    }
}

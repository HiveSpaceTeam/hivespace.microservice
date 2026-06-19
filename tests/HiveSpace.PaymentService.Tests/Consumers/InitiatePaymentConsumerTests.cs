using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.PaymentService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using MassTransit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Consumers;

public class InitiatePaymentConsumerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IPaymentGatewayFactory _gatewayFactory = Substitute.For<IPaymentGatewayFactory>();
    private readonly InitiatePaymentConsumer _consumer;

    public InitiatePaymentConsumerTests()
    {
        _consumer = new InitiatePaymentConsumer(_paymentRepository, _gatewayFactory);
    }

    [Fact]
    public async Task Consume_WhenIdempotentKeyExists_ReturnsExistingPayment()
    {
        var correlationId = Guid.NewGuid();
        var existing = Payment.CreateForOrder(
            Guid.NewGuid(), Guid.NewGuid(),
            HiveSpace.Domain.Shared.ValueObjects.Money.FromVND(100_000),
            HiveSpace.PaymentService.Domain.ValueObjects.PaymentMethod.BankTransfer("VNPAY"),
            PaymentGateway.VNPay,
            "idem-key-123");
        var msg = new InitiatePayment
        {
            CorrelationId = correlationId,
            OrderIds = [Guid.NewGuid()],
            BuyerId = Guid.NewGuid(),
            Amount = 100_000,
            Currency = "VND",
            Gateway = "VNPay",
            ReturnUrl = "https://example.com/return",
            CancelUrl = "https://example.com/cancel",
            IdempotencyKey = "idem-key-123"
        };
        var ctx = Substitute.For<ConsumeContext<InitiatePayment>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);
        _paymentRepository.GetByIdempotencyKeyAsync("idem-key-123", Arg.Any<CancellationToken>()).Returns(existing);

        await _consumer.Consume(ctx);

        await ctx.Received(1).RespondAsync<PaymentInitiatedIntegrationEvent>(Arg.Any<object>());
        _gatewayFactory.DidNotReceive().GetGateway(Arg.Any<PaymentGateway>());
    }

    [Fact]
    public async Task Consume_WhenNewPayment_InitiatesGatewayAndResponds()
    {
        var gateway = Substitute.For<IPaymentGateway>();
        gateway.InitiatePaymentAsync(Arg.Any<Payment>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GatewayInitiateResult("https://pay.vnpay.vn/abc", "txn-001"));
        _gatewayFactory.GetGateway(Arg.Any<PaymentGateway>()).Returns(gateway);
        _paymentRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);
        _paymentRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var msg = new InitiatePayment
        {
            CorrelationId = Guid.NewGuid(),
            OrderIds = [Guid.NewGuid()],
            BuyerId = Guid.NewGuid(),
            Amount = 200_000,
            Currency = "VND",
            Gateway = "VNPay",
            ReturnUrl = "https://example.com/return",
            CancelUrl = "https://example.com/cancel",
            IdempotencyKey = $"idem-{Guid.NewGuid()}"
        };
        var ctx = Substitute.For<ConsumeContext<InitiatePayment>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await ctx.Received(1).RespondAsync<PaymentInitiatedIntegrationEvent>(Arg.Any<object>());
        await _paymentRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenGatewayStringIsInvalid_DefaultsToVNPayAndSucceeds()
    {
        var gateway = Substitute.For<IPaymentGateway>();
        gateway.InitiatePaymentAsync(Arg.Any<Payment>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GatewayInitiateResult("https://pay.vnpay.vn/fallback", "txn-fallback"));
        _gatewayFactory.GetGateway(PaymentGateway.VNPay).Returns(gateway);
        _paymentRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);
        _paymentRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var msg = new InitiatePayment
        {
            CorrelationId = Guid.NewGuid(),
            OrderIds = [Guid.NewGuid()],
            BuyerId = Guid.NewGuid(),
            Amount = 100_000,
            Currency = "VND",
            Gateway = "UNKNOWN_GATEWAY",
            ReturnUrl = "https://example.com/return",
            CancelUrl = "https://example.com/cancel",
            IdempotencyKey = $"idem-{Guid.NewGuid()}"
        };
        var ctx = Substitute.For<ConsumeContext<InitiatePayment>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await ctx.Received(1).RespondAsync(Arg.Any<PaymentInitiatedIntegrationEvent>());
    }

    [Fact]
    public async Task Consume_WhenGatewayThrows_RespondsWithFailure()
    {
        var gateway = Substitute.For<IPaymentGateway>();
        gateway.InitiatePaymentAsync(Arg.Any<Payment>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Gateway unavailable"));
        _gatewayFactory.GetGateway(Arg.Any<PaymentGateway>()).Returns(gateway);
        _paymentRepository.GetByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var msg = new InitiatePayment
        {
            CorrelationId = Guid.NewGuid(),
            OrderIds = [Guid.NewGuid()],
            BuyerId = Guid.NewGuid(),
            Amount = 150_000,
            Currency = "VND",
            Gateway = "VNPay",
            ReturnUrl = "https://example.com/return",
            CancelUrl = "https://example.com/cancel",
            IdempotencyKey = $"idem-{Guid.NewGuid()}"
        };
        var ctx = Substitute.For<ConsumeContext<InitiatePayment>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await _consumer.Consume(ctx);

        await ctx.Received(1).RespondAsync(Arg.Any<PaymentInitiationFailedIntegrationEvent>());
        await _paymentRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

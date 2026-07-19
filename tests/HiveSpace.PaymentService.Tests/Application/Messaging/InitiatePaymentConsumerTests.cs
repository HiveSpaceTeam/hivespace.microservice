using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.PaymentService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.PaymentService.Application.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.External;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using HiveSpace.Domain.Shared.ValueObjects;
using MassTransit;
using NSubstitute;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Application.Messaging;

public class InitiatePaymentConsumerTests
{
    [Fact]
    public async Task Consume_MismatchedLinkedOrderTotal_PublishesPaymentInitiationFailed()
    {
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var currencyPolicyRepository = Substitute.For<IPlatformCurrencyPolicyRefRepository>();
        var gatewayFactory = Substitute.For<IPaymentGatewayFactory>();
        var referenceGenerator = Substitute.For<IPaymentReferenceNoGenerator>();
        currencyPolicyRepository.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(new PlatformCurrencyPolicyRef(Guid.NewGuid(), "VND", 1, DateTimeOffset.UtcNow, ["VND"]));
        var consumer = new InitiatePaymentConsumer(
            paymentRepository,
            currencyPolicyRepository,
            gatewayFactory,
            referenceGenerator);
        var message = new InitiatePayment
        {
            CorrelationId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            MethodCode = "VNPAY",
            Amount = 100_000,
            CurrencyCode = "VND",
            IdempotencyKey = "idem-key",
            Orders =
            [
                new CheckoutPaymentOrderDto(Guid.NewGuid(), "ORD-01JZXYZABCDEABCDEABCDEABC", Guid.NewGuid(), 60_000, "VND")
            ]
        };
        var context = Substitute.For<ConsumeContext<InitiatePayment>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await context.Received(1).RespondAsync(
            Arg.Is<PaymentInitiationFailedIntegrationEvent>(x =>
                x.CorrelationId == message.CorrelationId &&
                x.MethodCode == "VNPAY" &&
                x.Amount == 100_000 &&
                x.CurrencyCode == "VND" &&
                x.Orders.Count == 1));
        paymentRepository.DidNotReceive().Add(Arg.Any<HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment>());
    }

    [Fact]
    public async Task Consume_ReusedIdempotencyKeyWithDifferentShape_PublishesPaymentInitiationFailed()
    {
        var paymentRepository = Substitute.For<IPaymentRepository>();
        var currencyPolicyRepository = Substitute.For<IPlatformCurrencyPolicyRefRepository>();
        var gatewayFactory = Substitute.For<IPaymentGatewayFactory>();
        var referenceGenerator = Substitute.For<IPaymentReferenceNoGenerator>();
        var buyerId = Guid.NewGuid();
        var existingOrderId = Guid.NewGuid();
        var existing = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment.CreateCheckout(
            Guid.NewGuid(),
            "PAY-01JZXYZABCDEABCDEABCDEABC",
            buyerId,
            Money.FromVND(100_000),
            "VNPAY",
            "idem-key",
            [new PaymentLinkedOrder(existingOrderId, "ORD-01JZXYZABCDEABCDEABCDEABC", Guid.NewGuid(), 100_000, "VND")]);
        paymentRepository.GetByIdempotencyKeyAsync("idem-key", Arg.Any<CancellationToken>())
            .Returns(existing);

        var consumer = new InitiatePaymentConsumer(
            paymentRepository,
            currencyPolicyRepository,
            gatewayFactory,
            referenceGenerator);
        var message = new InitiatePayment
        {
            CorrelationId = Guid.NewGuid(),
            BuyerId = buyerId,
            MethodCode = "VNPAY",
            Amount = 120_000,
            CurrencyCode = "VND",
            IdempotencyKey = "idem-key",
            Orders =
            [
                new CheckoutPaymentOrderDto(existingOrderId, "ORD-01JZXYZABCDEABCDEABCDEABC", Guid.NewGuid(), 120_000, "VND")
            ]
        };
        var context = Substitute.For<ConsumeContext<InitiatePayment>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(context);

        await context.Received(1).RespondAsync(
            Arg.Is<PaymentInitiationFailedIntegrationEvent>(x =>
                x.CorrelationId == message.CorrelationId &&
                x.ReasonCode == "InvalidFieldException"));
        await context.DidNotReceive().RespondAsync(Arg.Any<PaymentInitiatedIntegrationEvent>());
        paymentRepository.DidNotReceive().Add(Arg.Any<HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment>());
    }
}

using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Domain.Shared.Enumerations;
using PaymentMethodVO = HiveSpace.PaymentService.Domain.ValueObjects.PaymentMethod;
using MassTransit;

namespace HiveSpace.PaymentService.Api.Consumers.Saga.CheckoutSaga;

public class InitiatePaymentConsumer(
    IPaymentRepository paymentRepository,
    IPaymentGatewayFactory gatewayFactory) : IConsumer<InitiatePayment>
{
    public async Task Consume(ConsumeContext<InitiatePayment> context)
    {
        var msg = context.Message;
        try
        {
            // Idempotency check
            var existing = await paymentRepository.GetByIdempotencyKeyAsync(msg.IdempotencyKey, context.CancellationToken);
            if (existing is not null)
            {
                await context.RespondAsync(new PaymentInitiated
                {
                    CorrelationId = msg.CorrelationId,
                    PaymentId = existing.Id,
                    PaymentUrl = existing.GatewayPaymentUrl ?? string.Empty,
                    ExpiresAt = existing.ExpiresAt
                });
                return;
            }

            if (!Enum.TryParse<PaymentGateway>(msg.Gateway, ignoreCase: true, out var gateway))
                gateway = PaymentGateway.VNPay;

            var currency = CurrencyExtensions.FromCode(msg.Currency);
            var amount = Money.FromVND(msg.Amount);

            var paymentMethod = PaymentMethodVO.BankTransfer("VNPAY");

            var payment = Domain.Aggregates.Payments.Payment.CreateForOrder(
                orderId: msg.OrderIds.FirstOrDefault(),
                buyerId: msg.BuyerId,
                amount: amount,
                paymentMethod: paymentMethod,
                gateway: gateway,
                idempotencyKey: msg.IdempotencyKey);

            paymentRepository.Add(payment);

            var gatewayImpl = gatewayFactory.GetGateway(gateway);
            var result = await gatewayImpl.InitiatePaymentAsync(
                payment, msg.ReturnUrl, msg.CancelUrl, context.CancellationToken);

            payment.MarkAsProcessing(result.PaymentUrl);
            await paymentRepository.SaveChangesAsync(context.CancellationToken);

            await context.RespondAsync(new PaymentInitiated
            {
                CorrelationId = msg.CorrelationId,
                PaymentId = payment.Id,
                PaymentUrl = result.PaymentUrl,
                ExpiresAt = payment.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            await context.RespondAsync(new PaymentInitiationFailed
            {
                CorrelationId = msg.CorrelationId,
                Reason = ex.Message
            });
        }
    }
}

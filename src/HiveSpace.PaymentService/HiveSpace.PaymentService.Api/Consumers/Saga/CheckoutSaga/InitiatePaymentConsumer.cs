using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;
using HiveSpace.PaymentService.Application.Payments;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Domain.Shared.Enumerations;
using PaymentMethodVO = HiveSpace.PaymentService.Domain.ValueObjects.PaymentMethod;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HiveSpace.PaymentService.Api.Consumers.Saga.CheckoutSaga;

public class InitiatePaymentConsumer(
    IPaymentRepository paymentRepository,
    IPlatformCurrencyPolicyRefRepository currencyPolicyRepository,
    IPaymentGatewayFactory gatewayFactory,
    IPaymentReferenceNoGenerator? referenceNoGenerator = null,
    ILogger<InitiatePaymentConsumer>? logger = null) : IConsumer<InitiatePayment>
{
    private readonly ILogger<InitiatePaymentConsumer> _logger = logger ?? NullLogger<InitiatePaymentConsumer>.Instance;

    public async Task Consume(ConsumeContext<InitiatePayment> context)
    {
        var msg = context.Message;
        var methodCode = GetMethodCode(msg);
        var currencyCode = GetCurrencyCode(msg);
        var orders = GetOrders(msg, currencyCode);
        try
        {
            // Idempotency check
            var existing = await paymentRepository.GetByIdempotencyKeyAsync(msg.IdempotencyKey, context.CancellationToken);
            if (existing is not null)
            {
                if (!MatchesRepeatedInitiation(existing, msg.BuyerId, methodCode, msg.Amount, currencyCode, orders))
                    throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrderTotalMismatch, nameof(msg.IdempotencyKey));

                await context.RespondAsync(new PaymentInitiatedIntegrationEvent
                {
                    CorrelationId = msg.CorrelationId,
                    PaymentId = existing.Id,
                    PaymentAttemptId = existing.CurrentAttemptId ?? Guid.Empty,
                    AttemptNo = existing.CurrentAttempt?.AttemptNo ?? 0,
                    ReferenceNo = existing.ReferenceNo ?? string.Empty,
                    BuyerId = existing.BuyerId,
                    MethodCode = existing.MethodCode,
                    Amount = existing.Amount.Amount,
                    CurrencyCode = existing.Amount.Currency.ToString(),
                    RedirectUrl = existing.GatewayPaymentUrl,
                    Orders = existing.LinkedOrders
                        .Select(o => new CheckoutPaymentOrderDto(o.OrderId, o.OrderCode, o.StoreId, o.Amount, o.CurrencyCode))
                        .ToList(),
                    PaymentUrl = existing.GatewayPaymentUrl ?? string.Empty,
                    ExpiresAt = existing.ExpiresAt
                });
                return;
            }

            ValidateLinkedOrders(msg.Amount, currencyCode, methodCode, orders);

            var gateway = methodCode == PaymentMethodCodes.Stripe
                ? PaymentGateway.Stripe
                : PaymentGateway.VNPay;

            var currency = CurrencyExtensions.FromCode(currencyCode);
            var currencyPolicy = await currencyPolicyRepository.GetCurrentAsync(context.CancellationToken)
                ?? throw new InvalidFieldException(PaymentDomainErrorCode.PlatformCurrencyPolicyMissing, nameof(currencyPolicyRepository));

            if (!currencyPolicy.IsCurrencyEnabled(currency.GetCode()))
                throw new InvalidFieldException(PaymentDomainErrorCode.PlatformCurrencyDisabled, nameof(msg.CurrencyCode));

            var amount = Money.FromSmallestUnit(msg.Amount, currencyCode);
            var referenceNo = referenceNoGenerator is null
                ? $"PAY-{Guid.NewGuid():N}"[..30].ToUpperInvariant()
                : await referenceNoGenerator.GenerateAsync(context.CancellationToken);

            var payment = Domain.Aggregates.Payments.Payment.CreateCheckout(
                msg.CorrelationId,
                referenceNo,
                msg.BuyerId,
                amount,
                methodCode,
                msg.IdempotencyKey,
                orders.Select(o => new Domain.Aggregates.Payments.PaymentLinkedOrder(
                    o.OrderId,
                    o.OrderCode,
                    o.StoreId,
                    o.Amount,
                    o.CurrencyCode)));

            paymentRepository.Add(payment);

            string? paymentUrl = null;
            if (methodCode != PaymentMethodCodes.COD)
            {
                var gatewayImpl = gatewayFactory.GetGateway(gateway);
                var result = await gatewayImpl.InitiatePaymentAsync(
                    payment, msg.ReturnUrl, msg.CancelUrl, context.CancellationToken);

                payment.MarkAsProcessing(result.PaymentUrl);
                paymentUrl = result.PaymentUrl;
            }
            await paymentRepository.SaveChangesAsync(context.CancellationToken);

            await context.RespondAsync(new PaymentInitiatedIntegrationEvent
            {
                CorrelationId = msg.CorrelationId,
                PaymentId = payment.Id,
                PaymentAttemptId = payment.CurrentAttemptId ?? Guid.Empty,
                AttemptNo = payment.CurrentAttempt?.AttemptNo ?? 0,
                ReferenceNo = payment.ReferenceNo ?? string.Empty,
                BuyerId = payment.BuyerId,
                MethodCode = payment.MethodCode,
                Amount = payment.Amount.Amount,
                CurrencyCode = payment.Amount.Currency.ToString(),
                RedirectUrl = paymentUrl,
                Orders = orders,
                PaymentUrl = paymentUrl ?? string.Empty,
                ExpiresAt = payment.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Checkout payment initiation failed for checkout {CorrelationId}, buyer {BuyerId}, method {MethodCode}, amount {Amount} {CurrencyCode}",
                msg.CorrelationId,
                msg.BuyerId,
                methodCode,
                msg.Amount,
                currencyCode);

            await context.RespondAsync(new PaymentInitiationFailedIntegrationEvent
            {
                CorrelationId = msg.CorrelationId,
                BuyerId = msg.BuyerId,
                MethodCode = methodCode,
                Amount = msg.Amount,
                CurrencyCode = currencyCode,
                ReasonCode = ex.GetType().Name,
                Reason = ex.Message,
                Orders = orders
            });
        }
    }

    private static string GetMethodCode(InitiatePayment msg)
    {
        var raw = string.IsNullOrWhiteSpace(msg.MethodCode) ? msg.Gateway : msg.MethodCode;
        if (string.IsNullOrWhiteSpace(raw))
            raw = PaymentMethodCodes.VNPAY;
        return raw.Equals(PaymentGateway.VNPay.ToString(), StringComparison.OrdinalIgnoreCase)
            ? PaymentMethodCodes.VNPAY
            : raw.Trim().ToUpperInvariant();
    }

    private static string GetCurrencyCode(InitiatePayment msg)
    {
        var raw = string.IsNullOrWhiteSpace(msg.CurrencyCode) ? msg.Currency : msg.CurrencyCode;
        return string.IsNullOrWhiteSpace(raw) ? Currency.VND.GetCode() : raw.Trim().ToUpperInvariant();
    }

    private static IReadOnlyList<CheckoutPaymentOrderDto> GetOrders(InitiatePayment msg, string currencyCode)
    {
        if (msg.Orders.Count > 0)
            return msg.Orders;

        if (msg.OrderIds.Count == 0)
            return [];

        var remaining = msg.Amount;
        var lastIndex = msg.OrderIds.Count - 1;
        return msg.OrderIds
            .Select((orderId, index) =>
            {
                var orderAmount = index == lastIndex
                    ? remaining
                    : msg.Amount / msg.OrderIds.Count;
                remaining -= orderAmount;
                return new CheckoutPaymentOrderDto(
                orderId,
                orderId.ToString(),
                Guid.NewGuid(),
                orderAmount,
                currencyCode);
            })
            .ToList();
    }

    private static void ValidateLinkedOrders(
        long amount,
        string currencyCode,
        string methodCode,
        IReadOnlyList<CheckoutPaymentOrderDto> orders)
    {
        if (methodCode == PaymentMethodCodes.Stripe)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentMethodInvalid, nameof(methodCode));
        if (orders.Count == 0)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrdersRequired, nameof(orders));
        if (orders.Select(o => o.OrderId).Distinct().Count() != orders.Count)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrderDuplicate, nameof(orders));
        if (orders.Any(o => !o.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase)) ||
            orders.Sum(o => o.Amount) != amount)
            throw new InvalidFieldException(PaymentDomainErrorCode.PaymentLinkedOrderTotalMismatch, nameof(orders));
    }

    private static bool MatchesRepeatedInitiation(
        Domain.Aggregates.Payments.Payment existing,
        Guid buyerId,
        string methodCode,
        long amount,
        string currencyCode,
        IReadOnlyList<CheckoutPaymentOrderDto> orders)
    {
        if (existing.BuyerId != buyerId ||
            !existing.MethodCode.Equals(methodCode, StringComparison.OrdinalIgnoreCase) ||
            existing.Amount.Amount != amount ||
            !existing.Amount.Currency.ToString().Equals(currencyCode, StringComparison.OrdinalIgnoreCase) ||
            existing.LinkedOrders.Count != orders.Count)
            return false;

        var existingOrders = existing.LinkedOrders
            .OrderBy(o => o.OrderId)
            .Select(o => new CheckoutPaymentOrderDto(o.OrderId, o.OrderCode, o.StoreId, o.Amount, o.CurrencyCode));

        var repeatedOrders = orders.OrderBy(o => o.OrderId);

        return existingOrders.SequenceEqual(repeatedOrders);
    }
}

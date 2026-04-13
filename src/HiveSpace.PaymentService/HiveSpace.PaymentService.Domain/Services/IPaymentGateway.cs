using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;

namespace HiveSpace.PaymentService.Domain.Services;

public interface IPaymentGateway
{
    PaymentGateway GatewayType { get; }

    Task<GatewayInitiateResult> InitiatePaymentAsync(
        Payment payment, string returnUrl, string cancelUrl, CancellationToken ct = default);

    Task<GatewayVerifyResult> VerifyWebhookAsync(
        Dictionary<string, string> payload, CancellationToken ct = default);
}

public record GatewayInitiateResult(string PaymentUrl, string TransactionRef);

public record GatewayVerifyResult(
    bool Success,
    string TransactionId,
    string RawResponse,
    string? ErrorMessage);

namespace HiveSpace.PaymentService.Application.Payments.Dtos;

public record PaymentDto(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId,
    long Amount,
    string Currency,
    string Status,
    string Gateway,
    string? GatewayTransactionId,
    string? GatewayPaymentUrl,
    DateTimeOffset? PaidAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt);

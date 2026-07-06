namespace HiveSpace.PaymentService.Application.Payments.Dtos;

public record PaymentDto(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId,
    PaymentMoneyDto Amount,
    string Status,
    string Gateway,
    string? GatewayTransactionId,
    string? GatewayPaymentUrl,
    DateTimeOffset? PaidAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt);

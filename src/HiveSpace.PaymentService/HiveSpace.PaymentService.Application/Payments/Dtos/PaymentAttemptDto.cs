namespace HiveSpace.PaymentService.Application.Payments.Dtos;

public record PaymentAttemptDto(
    Guid Id,
    int AttemptNo,
    string MethodCode,
    string? GatewayCode,
    string Status,
    string? RedirectUrl,
    string? GatewayTransactionId,
    string? FailureReasonCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? CompletedAt);

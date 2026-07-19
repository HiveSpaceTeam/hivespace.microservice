namespace HiveSpace.PaymentService.Api.Models;

public record CreatePaymentAttemptRequest(
    string MethodCode,
    string IdempotencyKey,
    string? ReturnUrl = null,
    string? CancelUrl = null);

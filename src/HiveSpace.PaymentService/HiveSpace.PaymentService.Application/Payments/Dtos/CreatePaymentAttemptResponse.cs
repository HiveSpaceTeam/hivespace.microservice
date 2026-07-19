namespace HiveSpace.PaymentService.Application.Payments.Dtos;

public record CreatePaymentAttemptResponse(
    Guid PaymentId,
    string ReferenceNo,
    PaymentAttemptDto Attempt);

using HiveSpace.Application.Shared.Commands;
using HiveSpace.PaymentService.Application.Payments.Dtos;

namespace HiveSpace.PaymentService.Application.Payments.Commands.CreatePaymentAttempt;

public record CreatePaymentAttemptCommand(
    Guid PaymentId,
    string MethodCode,
    string IdempotencyKey,
    string? ReturnUrl = null,
    string? CancelUrl = null) : ICommand<CreatePaymentAttemptResponse>;

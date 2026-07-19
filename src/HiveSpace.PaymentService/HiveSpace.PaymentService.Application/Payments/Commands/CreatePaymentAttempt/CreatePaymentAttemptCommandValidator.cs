using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.PaymentService.Application.Payments.Commands.CreatePaymentAttempt;

public class CreatePaymentAttemptCommandValidator : AbstractValidator<CreatePaymentAttemptCommand>
{
    public CreatePaymentAttemptCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreatePaymentAttemptCommand.PaymentId)));
        RuleFor(x => x.MethodCode)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreatePaymentAttemptCommand.MethodCode)));
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreatePaymentAttemptCommand.IdempotencyKey)));
    }
}

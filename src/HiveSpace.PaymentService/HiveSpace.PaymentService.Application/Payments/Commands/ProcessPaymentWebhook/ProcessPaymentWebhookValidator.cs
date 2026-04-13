using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;

public class ProcessPaymentWebhookValidator : AbstractValidator<ProcessPaymentWebhookCommand>
{
    public ProcessPaymentWebhookValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ProcessPaymentWebhookCommand.PaymentId)));

        RuleFor(x => x.Payload)
            .NotNull()
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ProcessPaymentWebhookCommand.Payload)));
    }
}

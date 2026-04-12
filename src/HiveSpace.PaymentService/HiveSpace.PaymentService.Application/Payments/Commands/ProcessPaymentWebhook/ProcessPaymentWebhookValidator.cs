using FluentValidation;

namespace HiveSpace.PaymentService.Application.Payments.Commands.ProcessPaymentWebhook;

public class ProcessPaymentWebhookValidator : AbstractValidator<ProcessPaymentWebhookCommand>
{
    public ProcessPaymentWebhookValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Payload).NotNull().NotEmpty();
    }
}

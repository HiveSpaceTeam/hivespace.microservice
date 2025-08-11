using FluentValidation;
using HiveSpace.OrderService.Application.Commands;

namespace HiveSpace.OrderService.Application.Validators;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status is invalid");
    }
}
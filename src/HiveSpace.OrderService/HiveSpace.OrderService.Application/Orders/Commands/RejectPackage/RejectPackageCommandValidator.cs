using FluentValidation;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Application.Orders.Commands.RejectPackage;

public class RejectPackageCommandValidator : AbstractValidator<RejectPackageCommand>
{
    public RejectPackageCommandValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithState(_ => new Error(OrderDomainErrorCode.PackageRejectionReasonRequired, nameof(RejectPackageCommand.Reason)));
    }
}

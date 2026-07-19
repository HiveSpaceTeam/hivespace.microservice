using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByReference;

public class GetPaymentByReferenceQueryValidator : AbstractValidator<GetPaymentByReferenceQuery>
{
    public GetPaymentByReferenceQueryValidator()
    {
        RuleFor(x => x.ReferenceNo)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(GetPaymentByReferenceQuery.ReferenceNo)));
    }
}

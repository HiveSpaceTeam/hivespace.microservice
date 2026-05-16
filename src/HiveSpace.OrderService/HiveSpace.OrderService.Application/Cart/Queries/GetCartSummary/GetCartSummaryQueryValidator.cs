using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCartSummary;

public class GetCartSummaryQueryValidator : AbstractValidator<GetCartSummaryQuery>
{
    public GetCartSummaryQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetCartSummaryQuery.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetCartSummaryQuery.PageSize)));
    }
}

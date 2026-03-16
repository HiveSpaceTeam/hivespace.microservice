using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCartItems;

public class GetCartItemsQueryValidator : AbstractValidator<GetCartItemsQuery>
{
    public GetCartItemsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetCartItemsQuery.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetCartItemsQuery.PageSize)));
    }
}

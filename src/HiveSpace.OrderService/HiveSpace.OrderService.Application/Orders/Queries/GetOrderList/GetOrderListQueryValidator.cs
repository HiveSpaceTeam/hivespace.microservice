using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;

public class GetOrderListQueryValidator : AbstractValidator<GetOrderListQuery>
{
    public GetOrderListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetOrderListQuery.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetOrderListQuery.PageSize)));
    }
}

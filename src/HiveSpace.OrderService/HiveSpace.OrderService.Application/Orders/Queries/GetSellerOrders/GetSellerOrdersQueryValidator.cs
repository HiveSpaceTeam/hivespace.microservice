using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

public class GetSellerOrdersQueryValidator : AbstractValidator<GetSellerOrdersQuery>
{
    public GetSellerOrdersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetSellerOrdersQuery.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetSellerOrdersQuery.PageSize)));
    }
}

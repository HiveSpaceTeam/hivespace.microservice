using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.OrderService.Application.Orders.Enums;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;

public class GetOrderListQueryValidator : AbstractValidator<GetOrderListQuery>
{
    private static readonly string[] SupportedSearchFields =
    [
        BuyerSearchField.OrderCode,
        BuyerSearchField.Product
    ];

    public GetOrderListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetOrderListQuery.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetOrderListQuery.PageSize)));

        RuleFor(x => x.SearchField)
            .Must(f => SupportedSearchFields.Contains(f, StringComparer.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.SearchField))
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(GetOrderListQuery.SearchField)));

        RuleFor(x => x.SearchValue)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.SearchField))
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(GetOrderListQuery.SearchValue)));

        RuleFor(x => x.SearchField)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.SearchValue))
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(GetOrderListQuery.SearchField)));
    }
}

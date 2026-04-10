using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.OrderService.Application.Orders.Enums;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

public class GetSellerOrdersQueryValidator : AbstractValidator<GetSellerOrdersQuery>
{
    private static readonly string[] SupportedSearchFields =
    [
        SellerSearchField.OrderCode,
        SellerSearchField.Product,
        SellerSearchField.CustomerName
    ];

    public GetSellerOrdersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageNumber, nameof(GetSellerOrdersQuery.Page)));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithState(_ => new Error(CommonErrorCode.InvalidPageSize, nameof(GetSellerOrdersQuery.PageSize)));

        RuleFor(x => x.SearchField)
            .Must(f => SupportedSearchFields.Contains(f, StringComparer.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.SearchField))
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(GetSellerOrdersQuery.SearchField)));

        RuleFor(x => x.SearchValue)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.SearchField))
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(GetSellerOrdersQuery.SearchValue)));

        RuleFor(x => x.SearchField)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.SearchValue))
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(GetSellerOrdersQuery.SearchField)));
    }
}

using HiveSpace.Application.Shared.Queries;
namespace HiveSpace.OrderService.Application.Cart.Queries.GetCartSummary;

public record GetCartSummaryQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<GetCartSummaryResponse>;

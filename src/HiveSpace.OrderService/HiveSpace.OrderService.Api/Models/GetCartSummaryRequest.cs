namespace HiveSpace.OrderService.Api.Models;

public record GetCartSummaryRequest(
    int Page = 1,
    int PageSize = 20
);

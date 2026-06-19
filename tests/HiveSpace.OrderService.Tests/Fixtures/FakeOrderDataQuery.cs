using HiveSpace.Core.Models.Pagination;
using HiveSpace.OrderService.Application.Orders;
using HiveSpace.OrderService.Application.Orders.Enums;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

namespace HiveSpace.OrderService.Tests.Fixtures;

public sealed class FakeOrderDataQuery : IOrderDataQuery
{
    public Task<GetOrderListResponse> GetPagedOrdersAsync(
        Guid userId, int page, int pageSize,
        BuyerOrderProcessStatus? processStatus,
        string? searchField, string? searchValue,
        CancellationToken ct = default)
        => Task.FromResult(new GetOrderListResponse([], new PaginationMetadata(page, pageSize, 0)));

    public Task<GetSellerOrdersResponse> GetSellerOrdersAsync(
        Guid storeId, int page, int pageSize,
        SellerOrderProcessStatus? processStatus,
        string? searchField, string? searchValue,
        CancellationToken ct = default)
        => Task.FromResult(new GetSellerOrdersResponse([], new PaginationMetadata(page, pageSize, 0)));
}

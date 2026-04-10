using HiveSpace.OrderService.Application.Orders.Enums;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;

namespace HiveSpace.OrderService.Application.Orders;

public interface IOrderDataQuery
{
    Task<GetOrderListResponse> GetPagedOrdersAsync(
        Guid userId, int page, int pageSize,
        CustomerOrderProcessStatus? processStatus,
        string? searchField, string? searchValue,
        CancellationToken ct = default);

    Task<GetSellerOrdersResponse> GetSellerOrdersAsync(
        Guid storeId, int page, int pageSize,
        SellerOrderProcessStatus? processStatus,
        string? searchField, string? searchValue,
        CancellationToken ct = default);
}

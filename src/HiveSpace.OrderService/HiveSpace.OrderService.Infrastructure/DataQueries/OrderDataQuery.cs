using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Orders;
using HiveSpace.OrderService.Application.Orders.Enums;
using HiveSpace.OrderService.Application.Orders.Mappers;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.DataQueries;

public class OrderDataQuery(IDbContextFactory<OrderDbContext> dbFactory) : IOrderDataQuery
{
    public async Task<GetOrderListResponse> GetPagedOrdersAsync(
        Guid userId, int page, int pageSize,
        CustomerOrderProcessStatus? processStatus,
        string? searchField, string? searchValue,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.Orders.AsNoTracking().Where(o => o.UserId == userId);

        var statuses = MapCustomerProcessStatus(processStatus);
        if (statuses.Length > 0)
            query = query.Where(o => statuses.Contains(o.Status));

        if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(searchValue))
            query = ApplyCustomerSearch(query, searchField, searchValue);

        var total = await query.CountAsync(ct);

        var orders = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new GetOrderListResponse(
            orders.Select(o => o.ToCustomerSummaryDto()).ToList(),
            new PaginationMetadata(page, pageSize, total));
    }

    public async Task<GetSellerOrdersResponse> GetSellerOrdersAsync(
        Guid storeId, int page, int pageSize,
        SellerOrderProcessStatus? processStatus,
        string? searchField, string? searchValue,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.Orders.AsNoTracking().Where(o => o.StoreId == storeId);

        var statuses = MapSellerProcessStatus(processStatus);
        if (statuses.Length > 0)
            query = query.Where(o => statuses.Contains(o.Status));

        if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(searchValue))
            query = ApplySellerSearch(query, searchField, searchValue);

        var total = await query.CountAsync(ct);

        var orders = await query
            .Include(o => o.Items)
            .Include(o => o.Checkouts.OrderByDescending(c => c.CreatedAt).Take(1))
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new GetSellerOrdersResponse(
            orders.Select(o => o.ToSellerSummaryDto()).ToList(),
            new PaginationMetadata(page, pageSize, total));
    }

    private static IQueryable<Order> ApplyCustomerSearch(
        IQueryable<Order> query, string field, string value) => field switch
    {
        var f when f.Equals(CustomerSearchField.OrderCode, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.ShortId.Contains(value)),
        var f when f.Equals(CustomerSearchField.Product, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.Items.Any(i => i.ProductSnapshot.ProductName.Contains(value))),
        var f when f.Equals(CustomerSearchField.StoreName, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.Items.Any(i => i.ProductSnapshot.StoreName.Contains(value))),
        _ => throw new InvalidFieldException(CommonErrorCode.InvalidArgument, nameof(GetOrderListQuery.SearchField))
    };

    private static IQueryable<Order> ApplySellerSearch(
        IQueryable<Order> query, string field, string value) => field switch
    {
        var f when f.Equals(SellerSearchField.OrderCode, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.ShortId.Contains(value)),
        var f when f.Equals(SellerSearchField.Product, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.Items.Any(i => i.ProductSnapshot.ProductName.Contains(value))),
        var f when f.Equals(SellerSearchField.CustomerName, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.DeliveryAddress.RecipientName.Contains(value)),
        _ => throw new InvalidFieldException(CommonErrorCode.InvalidArgument, nameof(GetSellerOrdersQuery.SearchField))
    };

    private static OrderStatus[] MapCustomerProcessStatus(CustomerOrderProcessStatus? status) => status switch
    {
        null or CustomerOrderProcessStatus.All => [],
        CustomerOrderProcessStatus.WaitingPayment => [OrderStatus.Created],
        CustomerOrderProcessStatus.Processing     => [OrderStatus.Paid, OrderStatus.COD, OrderStatus.Confirmed, OrderStatus.ReadyToShip],
        CustomerOrderProcessStatus.Shipping       => [OrderStatus.Shipped],
        CustomerOrderProcessStatus.Delivered      => [OrderStatus.Delivered, OrderStatus.Completed],
        CustomerOrderProcessStatus.Cancelled      => [OrderStatus.Cancelled, OrderStatus.Rejected, OrderStatus.Expired],
        CustomerOrderProcessStatus.ReturnRefund   => [OrderStatus.Refunding, OrderStatus.Refunded, OrderStatus.Solved, OrderStatus.Claimed],
        _ => throw new InvalidFieldException(CommonErrorCode.InvalidStatusFilter, nameof(GetOrderListQuery.ProcessStatus))
    };

    private static OrderStatus[] MapSellerProcessStatus(SellerOrderProcessStatus? status) => status switch
    {
        null or SellerOrderProcessStatus.All => [],
        SellerOrderProcessStatus.PendingConfirmation => [OrderStatus.Paid, OrderStatus.COD],
        SellerOrderProcessStatus.ReadyToShip         => [OrderStatus.Confirmed, OrderStatus.ReadyToShip],
        SellerOrderProcessStatus.Shipping            => [OrderStatus.Shipped],
        SellerOrderProcessStatus.Delivered           => [OrderStatus.Delivered, OrderStatus.Completed],
        SellerOrderProcessStatus.ReturnCancel        => [OrderStatus.Cancelled, OrderStatus.Rejected, OrderStatus.Expired, OrderStatus.Refunding, OrderStatus.Refunded, OrderStatus.Solved, OrderStatus.Claimed],
        _ => throw new InvalidFieldException(CommonErrorCode.InvalidStatusFilter, nameof(GetSellerOrdersQuery.ProcessStatus))
    };
}

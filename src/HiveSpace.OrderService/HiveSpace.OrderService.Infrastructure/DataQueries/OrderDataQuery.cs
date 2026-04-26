using HiveSpace.Core.Models.Pagination;
using HiveSpace.Domain.Shared.Errors;
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
        BuyerOrderProcessStatus? processStatus,
        string? searchField, string? searchValue,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.Orders.AsNoTracking().Where(o => o.UserId == userId);

        var statuses = MapBuyerProcessStatus(processStatus);
        if (statuses.Length > 0)
            query = query.Where(o => statuses.Contains(o.Status));

        if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(searchValue))
            query = ApplyBuyerSearch(query, searchField, searchValue);

        var total = await query.CountAsync(ct);

        var orders = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new GetOrderListResponse(
            orders.Select(o => o.ToBuyerSummaryDto()).ToList(),
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

    private static IQueryable<Order> ApplyBuyerSearch(
        IQueryable<Order> query, string field, string value) => field switch
    {
        var f when f.Equals(BuyerSearchField.OrderCode, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.OrderCode.Contains(value)),
        var f when f.Equals(BuyerSearchField.Product, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.Items.Any(i => i.ProductSnapshot.ProductName.Contains(value))),
        _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(GetOrderListQuery.SearchField))
    };

    private static IQueryable<Order> ApplySellerSearch(
        IQueryable<Order> query, string field, string value) => field switch
    {
        var f when f.Equals(SellerSearchField.OrderCode, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.OrderCode.Contains(value)),
        var f when f.Equals(SellerSearchField.Product, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.Items.Any(i => i.ProductSnapshot.ProductName.Contains(value))),
        var f when f.Equals(SellerSearchField.BuyerName, StringComparison.OrdinalIgnoreCase)
            => query.Where(o => o.DeliveryAddress.RecipientName.Contains(value)),
        _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(GetSellerOrdersQuery.SearchField))
    };

    private static OrderStatus[] MapBuyerProcessStatus(BuyerOrderProcessStatus? status) => status switch
    {
        null or BuyerOrderProcessStatus.All => [],
        BuyerOrderProcessStatus.WaitingPayment => [OrderStatus.Created],
        BuyerOrderProcessStatus.Processing     => [OrderStatus.Paid, OrderStatus.COD, OrderStatus.Confirmed, OrderStatus.ReadyToShip],
        BuyerOrderProcessStatus.Shipping       => [OrderStatus.Shipped],
        BuyerOrderProcessStatus.Delivered      => [OrderStatus.Delivered, OrderStatus.Completed],
        BuyerOrderProcessStatus.Cancelled      => [OrderStatus.Cancelled, OrderStatus.Rejected, OrderStatus.Expired],
        BuyerOrderProcessStatus.ReturnRefund   => [OrderStatus.Refunding, OrderStatus.Refunded, OrderStatus.Solved, OrderStatus.Claimed],
        _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(GetOrderListQuery.ProcessStatus))
    };

    private static OrderStatus[] MapSellerProcessStatus(SellerOrderProcessStatus? status) => status switch
    {
        null or SellerOrderProcessStatus.All => [],
        SellerOrderProcessStatus.PendingConfirmation => [OrderStatus.Paid, OrderStatus.COD],
        SellerOrderProcessStatus.ReadyToShip         => [OrderStatus.Confirmed, OrderStatus.ReadyToShip],
        SellerOrderProcessStatus.Shipping            => [OrderStatus.Shipped],
        SellerOrderProcessStatus.Delivered           => [OrderStatus.Delivered, OrderStatus.Completed],
        SellerOrderProcessStatus.ReturnedOrCancelled  => [OrderStatus.Cancelled, OrderStatus.Rejected, OrderStatus.Expired, OrderStatus.Refunding, OrderStatus.Refunded, OrderStatus.Solved, OrderStatus.Claimed],
        _ => throw new InvalidFieldException(DomainErrorCode.InvalidEnumerationValue, nameof(GetSellerOrdersQuery.ProcessStatus))
    };
}

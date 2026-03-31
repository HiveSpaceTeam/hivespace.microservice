using HiveSpace.OrderService.Application.Orders;
using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Application.Orders.Mappers;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;
using HiveSpace.OrderService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.DataQueries;

public class OrderDataQuery(IDbContextFactory<OrderDbContext> dbFactory) : IOrderDataQuery
{
    public async Task<GetOrderListResponse> GetPagedOrdersAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.Orders.AsNoTracking().Where(o => o.UserId == userId);

        var total = await query.CountAsync(ct);

        var orders = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new GetOrderListResponse(
            orders.Select(o => o.ToSummaryDto()).ToList(),
            total, page, pageSize);
    }

    public async Task<GetSellerOrdersResponse> GetSellerOrdersAsync(
        Guid storeId, int page, int pageSize, string? status, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.Orders.AsNoTracking().Where(o => o.StoreId == storeId);

        if (status is not null)
            query = query.Where(o => EF.Property<string>(o, "Status") == status);

        var total = await query.CountAsync(ct);

        var orders = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new SellerOrderSummaryDto
            {
                Id          = o.Id,
                StoreId     = o.StoreId,
                Status      = o.Status.Name,
                SubTotal    = o.SubTotal.Amount,
                TotalAmount = o.TotalAmount.Amount,
                Currency    = o.TotalAmount.Currency.ToString(),
                ItemCount   = o.Items.Count,
                CreatedAt   = o.CreatedAt
            })
            .ToListAsync(ct);

        return new GetSellerOrdersResponse(orders, total, page, pageSize);
    }
}

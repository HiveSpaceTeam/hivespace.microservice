using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Application.Orders.Mappers;
using HiveSpace.OrderService.Application.Orders.Queries.GetOrderList;
using HiveSpace.OrderService.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.QueryHandlers;

public class GetOrderListQueryHandler(OrderDbContext db, IUserContext userContext)
    : IRequestHandler<GetOrderListQuery, GetOrderListResponse>
{
    public async Task<GetOrderListResponse> Handle(GetOrderListQuery request, CancellationToken cancellationToken)
    {
        var query = db.Orders.AsNoTracking().Where(o => o.UserId == userContext.UserId);

        var total = await query.CountAsync(cancellationToken);

        var orders = await query
            .Include(o => o.Packages)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetOrderListResponse(
            orders.Select(o => o.ToSummaryDto()).ToList(),
            total, request.Page, request.PageSize);
    }
}

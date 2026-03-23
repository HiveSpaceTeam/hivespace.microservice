using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerPackages;
using HiveSpace.OrderService.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.QueryHandlers;

public class GetSellerPackagesQueryHandler(OrderDbContext db, IUserContext userContext)
    : IRequestHandler<GetSellerPackagesQuery, GetSellerPackagesResponse>
{
    public async Task<GetSellerPackagesResponse> Handle(GetSellerPackagesQuery request, CancellationToken cancellationToken)
    {
        var storeId = userContext.StoreId ?? Guid.Empty;

        var query = db.OrderPackages.AsNoTracking().Where(p => p.StoreId == storeId);

        if (request.Status is not null)
            query = query.Where(p => EF.Property<string>(p, "Status") == request.Status);

        var total = await query.CountAsync(cancellationToken);

        var packages = await query
            .Include(p => p.Items)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PackageSummaryDto
            {
                Id          = p.Id,
                OrderId     = EF.Property<Guid>(p, "OrderId"),
                StoreId     = p.StoreId,
                Status      = p.Status.Name,
                SubTotal    = p.SubTotal.Amount,
                TotalAmount = p.TotalAmount.Amount,
                Currency    = p.TotalAmount.Currency.ToString(),
                ItemCount   = p.Items.Count,
                CreatedAt   = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetSellerPackagesResponse(packages, total, request.Page, request.PageSize);
    }
}

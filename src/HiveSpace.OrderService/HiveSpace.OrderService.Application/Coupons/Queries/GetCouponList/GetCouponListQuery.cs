using HiveSpace.Application.Shared.Queries;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Coupons.Queries.GetCouponList;

public record GetCouponListQuery(
    CouponStatus? CouponStatus,
    string? Keyword,
    int Page,
    int PageSize,
    string? CouponName = null,
    string? CouponCode = null
) : IQuery<GetCouponListResponse>;

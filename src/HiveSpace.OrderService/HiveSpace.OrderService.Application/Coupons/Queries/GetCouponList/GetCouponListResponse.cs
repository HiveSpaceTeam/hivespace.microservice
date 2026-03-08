using System.Collections.Generic;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.OrderService.Application.Coupons.Dtos;

namespace HiveSpace.OrderService.Application.Coupons.Queries.GetCouponList;

public record GetCouponListResponse(
    List<CouponSummaryDto> Coupons,
    PaginationMetadata Pagination
);

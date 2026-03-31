using HiveSpace.Application.Shared.Queries;
using HiveSpace.OrderService.Application.Coupons.Dtos;

namespace HiveSpace.OrderService.Application.Coupons.Queries.GetCouponById;

public record GetCouponByIdQuery(Guid Id) : IQuery<CouponDto>;

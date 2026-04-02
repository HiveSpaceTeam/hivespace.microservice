using HiveSpace.Application.Shared.Commands;
using HiveSpace.OrderService.Application.Coupons.Dtos;

namespace HiveSpace.OrderService.Application.Coupons.Commands.EndCoupon;

public record EndCouponCommand(Guid Id) : ICommand<CouponDto>;

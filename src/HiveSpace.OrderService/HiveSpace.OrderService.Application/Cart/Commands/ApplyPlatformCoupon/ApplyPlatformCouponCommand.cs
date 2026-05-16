using HiveSpace.Application.Shared.Commands;
using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Application.Cart.Commands.ApplyPlatformCoupon;

public record ApplyPlatformCouponCommand(string CouponCode) : ICommand<AppliedPlatformCouponDto>;

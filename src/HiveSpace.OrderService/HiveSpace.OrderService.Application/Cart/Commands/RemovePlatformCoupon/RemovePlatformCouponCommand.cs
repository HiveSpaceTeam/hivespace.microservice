using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.OrderService.Application.Cart.Commands.RemovePlatformCoupon;

public record RemovePlatformCouponCommand(string CouponCode) : ICommand;

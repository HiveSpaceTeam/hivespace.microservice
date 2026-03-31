using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.OrderService.Application.Coupons.Commands.DeleteCoupon;

public record DeleteCouponCommand(Guid Id) : ICommand;

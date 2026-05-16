using HiveSpace.Application.Shared.Commands;
using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Application.Cart.Commands.ApplyStoreCoupon;

public record ApplyStoreCouponCommand(Guid StoreId, string CouponCode) : ICommand<AppliedStoreCouponDto>;

using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.OrderService.Application.Cart.Commands.RemoveStoreCoupon;

public record RemoveStoreCouponCommand(Guid StoreId) : ICommand;

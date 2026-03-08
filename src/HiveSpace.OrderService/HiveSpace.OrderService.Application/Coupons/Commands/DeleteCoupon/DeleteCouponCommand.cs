using MediatR;

namespace HiveSpace.OrderService.Application.Coupons.Commands.DeleteCoupon;

public record DeleteCouponCommand(Guid Id) : IRequest;

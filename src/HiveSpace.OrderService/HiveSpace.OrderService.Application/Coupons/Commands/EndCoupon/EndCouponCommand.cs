using System;
using MediatR;
using HiveSpace.OrderService.Application.Coupons.Dtos;

namespace HiveSpace.OrderService.Application.Coupons.Commands.EndCoupon;

public record EndCouponCommand(Guid Id) : IRequest<CouponDto>;

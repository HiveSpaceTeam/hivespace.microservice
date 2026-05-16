using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.ApplyPlatformCoupon;

public class ApplyPlatformCouponCommandHandler(
    ICartRepository cartRepository,
    ICheckoutQuery checkoutQuery,
    ICouponRepository couponRepository,
    IUserContext userContext)
    : ICommandHandler<ApplyPlatformCouponCommand, AppliedPlatformCouponDto>
{
    public async Task<AppliedPlatformCouponDto> Handle(ApplyPlatformCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(userContext.UserId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartNotFound, nameof(Cart));

        var selectedCart = await checkoutQuery.GetSelectedCartItemsAsync(userContext.UserId, cancellationToken);
        SelectedCartCouponEvaluator.EnsureSelectedCartExists(selectedCart, nameof(ApplyPlatformCouponCommandHandler));

        var coupon = (await couponRepository.GetByCodesAsync([request.CouponCode], cancellationToken))
            .FirstOrDefault(c => c.Code.Equals(request.CouponCode, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(OrderDomainErrorCode.CouponNotFound, nameof(Coupon));

        if (coupon.OwnerType != CouponOwnerType.Platform)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponStoreNotApplicable, nameof(request.CouponCode));

        var grandSubtotal = SelectedCartCouponEvaluator.BuildStoreSnapshots(selectedCart).Sum(x => x.Subtotal);
        var validation = coupon.Validate(userContext.UserId, HiveSpace.Domain.Shared.ValueObjects.Money.FromVND(grandSubtotal));
        if (!validation.IsValid)
            throw new InvalidFieldException(validation.Errors.First().ErrorCode, nameof(request.CouponCode));

        cart.ApplyPlatformCoupon(coupon.Code);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return new AppliedPlatformCouponDto(coupon.Code);
    }
}

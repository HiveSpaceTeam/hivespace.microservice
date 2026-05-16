using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Cart.Commands.ApplyStoreCoupon;

public class ApplyStoreCouponCommandHandler(
    ICartRepository cartRepository,
    ICheckoutQuery checkoutQuery,
    ICouponRepository couponRepository,
    IUserContext userContext)
    : ICommandHandler<ApplyStoreCouponCommand, AppliedStoreCouponDto>
{
    public async Task<AppliedStoreCouponDto> Handle(ApplyStoreCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(userContext.UserId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartNotFound, nameof(Cart));

        var selectedCart = await checkoutQuery.GetSelectedCartItemsAsync(userContext.UserId, cancellationToken);
        var snapshot = SelectedCartCouponEvaluator.GetStoreSnapshot(selectedCart, request.StoreId, nameof(ApplyStoreCouponCommandHandler));

        var coupon = (await couponRepository.GetByCodesAsync([request.CouponCode], cancellationToken))
            .FirstOrDefault(c => c.Code.Equals(request.CouponCode, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(OrderDomainErrorCode.CouponNotFound, nameof(Coupon));

        if (coupon.OwnerType != CouponOwnerType.Store || coupon.StoreId != request.StoreId)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponStoreNotApplicable, nameof(request.CouponCode));

        var evaluation = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, userContext.UserId, snapshot);
        if (!evaluation.IsApplicable)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalid, nameof(request.CouponCode));

        cart.ApplyStoreCoupon(request.StoreId, coupon.Code);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return new AppliedStoreCouponDto(request.StoreId, coupon.Code);
    }
}

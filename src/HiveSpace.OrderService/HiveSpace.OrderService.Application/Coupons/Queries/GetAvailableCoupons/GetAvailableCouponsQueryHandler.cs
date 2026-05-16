using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Specifications;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Coupons.Queries.GetAvailableCoupons;

public class GetAvailableCouponsQueryHandler(
    ICheckoutQuery checkoutQuery,
    ICouponRepository couponRepository,
    IStoreRefRepository storeRefRepository,
    IUserContext userContext)
    : IQueryHandler<GetAvailableCouponsQuery, GetAvailableCouponsResponse>
{
    public async Task<GetAvailableCouponsResponse> Handle(GetAvailableCouponsQuery request, CancellationToken cancellationToken)
    {
        var selectedCart = await checkoutQuery.GetSelectedCartItemsAsync(userContext.UserId, cancellationToken);
        var snapshot = SelectedCartCouponEvaluator.GetStoreSnapshot(
            selectedCart,
            request.StoreId,
            nameof(GetAvailableCouponsQueryHandler));
        var evaluationSnapshot = SelectedCartCouponEvaluator.FilterSnapshotByProductIds(snapshot, request.ProductIds);
        var store = await storeRefRepository.GetByIdAsync(request.StoreId, cancellationToken);
        var storeName = store?.Name ?? snapshot.StoreName ?? string.Empty;
        var storeLogoUrl = store?.LogoUrl;

        Specification<Coupon> specification = new CouponOwnedByStoreSpecification(request.StoreId)
            .And(new CouponOngoingSpecification())
            .And(c => !c.IsHidden);

        var coupons = await couponRepository.GetListWithUsagesAsync(specification, cancellationToken);

        return new GetAvailableCouponsResponse(
            request.StoreId,
            storeName,
            storeLogoUrl,
            coupons
                .Where(coupon =>
                {
                    var evaluation = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, userContext.UserId, evaluationSnapshot);
                    return !ShouldHideCoupon(evaluation);
                })
                .Select(coupon =>
                {
                    var evaluation = SelectedCartCouponEvaluator.EvaluateCoupon(coupon, userContext.UserId, evaluationSnapshot);
                    return new AvailableCouponDto
                    {
                        Id = coupon.Id,
                        Code = coupon.Code,
                        Name = coupon.Name,
                        StartDateTime = coupon.StartDateTime,
                        EndDateTime = coupon.EndDateTime,
                        DiscountType = coupon.DiscountType,
                        DiscountAmount = coupon.DiscountAmount?.Amount,
                        DiscountCurrency = coupon.DiscountAmount?.Currency.GetCode() ?? coupon.MinOrderAmount.Currency.GetCode(),
                        DiscountPercentage = coupon.DiscountType == Domain.Enumerations.DiscountType.Percentage ? coupon.DiscountPercentage : null,
                        MaxDiscountAmount = coupon.MaxDiscountAmount?.Amount,
                        MinOrderAmount = coupon.MinOrderAmount.Amount,
                        Scope = coupon.Scope,
                        IsApplicable = evaluation.IsApplicable
                    };
                })
                .ToList());
    }

    private static bool ShouldHideCoupon(CouponEvaluationResult evaluation)
        => !evaluation.IsApplicable &&
           evaluation.Errors.Any(error =>
               error == OrderDomainErrorCode.CouponUsageLimitReached ||
               error == OrderDomainErrorCode.CouponUserLimitReached);
}

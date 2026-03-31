using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Application.Coupons.Mappers;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandHandler : ICommandHandler<CreateCouponCommand, CouponDto>
{
    private readonly ICouponRepository _couponRepository;
    private readonly IUserContext _userContext;

    public CreateCouponCommandHandler(ICouponRepository couponRepository, IUserContext userContext)
    {
        _couponRepository = couponRepository;
        _userContext = userContext;
    }

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        // Map Enums
        var discountType = request.DiscountType;
        var scope = request.Scope;
        
        // Infer OwnerType from User Role
        var ownerType = _userContext.IsSeller ? CouponOwnerType.Store : CouponOwnerType.Platform;

        // Map Money objects
        Money? discountAmount = request.DiscountType == DiscountType.FixedAmount && request.DiscountAmount.HasValue && !string.IsNullOrEmpty(request.DiscountCurrency)
            ? Money.Create(request.DiscountAmount.Value, request.DiscountCurrency)
            : null;

        Money? maxDiscountAmount = request.MaxDiscountAmount.HasValue && !string.IsNullOrEmpty(request.DiscountCurrency)
            ? Money.Create(request.MaxDiscountAmount.Value, request.DiscountCurrency)
            : null;

        Money minOrderAmount = !string.IsNullOrEmpty(request.DiscountCurrency)
            ? Money.Create(request.MinOrderAmount, request.DiscountCurrency)
            : Money.Zero(); 

        // Get CreatorId from Context
        var creatorId = _userContext.UserId; 

        Coupon coupon;

        if (ownerType == CouponOwnerType.Platform)
        {
            coupon = Coupon.CreateByPlatform(
                creatorId.ToString(),
                request.Code,
                request.Name,
                discountType,
                request.DiscountPercentage,
                discountAmount,
                scope,
                request.StartDateTime,
                request.EndDateTime,
                request.EarlySaveDateTime,
                request.IsHidden,
                maxDiscountAmount,
                minOrderAmount
            );
        }
        else
        {
            var storeId = _userContext.StoreId.GetValueOrDefault();

            coupon = Coupon.CreateByStore(
                storeId,
                creatorId,
                request.Code,
                request.Name,
                discountType,
                request.DiscountPercentage,
                discountAmount,
                scope,
                request.StartDateTime,
                request.EndDateTime,
                request.EarlySaveDateTime,
                request.IsHidden,
                maxDiscountAmount,
                minOrderAmount
            );
        }

        if (request.MaxUsageCount > 0)
            coupon.SetMaxUsageCount(request.MaxUsageCount);

        if (request.MaxUsagePerUser > 0)
            coupon.SetMaxUsagePerUser(request.MaxUsagePerUser);
            
        if (request.ApplicableProductIds.Count > 0)
            coupon.LimitToProducts(request.ApplicableProductIds);

        if (request.ApplicableCategoryIds.Count > 0)
            coupon.LimitToCategories(request.ApplicableCategoryIds);

        _couponRepository.Add(coupon);
        await _couponRepository.SaveChangesAsync(cancellationToken);

        var responseDto = coupon.ToDto();

        return responseDto;
    }
}

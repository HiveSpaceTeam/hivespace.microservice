using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Application.Coupons.Mappers;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommandHandler : ICommandHandler<UpdateCouponCommand, CouponDto>
{
    private readonly ICouponRepository _couponRepository;
    private readonly IUserContext _userContext;
    private readonly IPlatformCurrencyPolicyRefRepository _currencyPolicyRepository;

    public UpdateCouponCommandHandler(ICouponRepository couponRepository, IUserContext userContext, IPlatformCurrencyPolicyRefRepository currencyPolicyRepository)
    {
        _couponRepository = couponRepository;
        _userContext = userContext;
        _currencyPolicyRepository = currencyPolicyRepository;
    }

    public async Task<CouponDto> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var currencyPolicy = await _currencyPolicyRepository.GetCurrentAsync(cancellationToken)
            ?? throw new InvalidFieldException(OrderDomainErrorCode.PlatformCurrencyPolicyMissing, nameof(_currencyPolicyRepository));

        if (!currencyPolicy.IsCurrencyEnabled(request.CurrencyCode))
            throw new InvalidFieldException(OrderDomainErrorCode.PlatformCurrencyDisabled, nameof(request.CurrencyCode));

        var coupon = await _couponRepository.GetByIdAsync(request.Id, true, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CouponNotFound, request.Id.ToString());

        // Explicit Authorization Check
        if (_userContext.IsSeller && _userContext.StoreId.HasValue && 
            !new CouponOwnedByStoreSpecification(_userContext.StoreId.Value).IsSatisfiedBy(coupon))
        {
             throw new ForbiddenException(OrderDomainErrorCode.CouponNotStoreOwned, request.Id.ToString());
        }

        Money? discountAmount = request.DiscountAmount.HasValue && !string.IsNullOrEmpty(request.CurrencyCode)
            ? Money.Create(request.DiscountAmount.Value, request.CurrencyCode)
            : null;

        Money? maxDiscountAmount = request.MaxDiscountAmount.HasValue && !string.IsNullOrEmpty(request.CurrencyCode)
            ? Money.Create(request.MaxDiscountAmount.Value, request.CurrencyCode)
            : null;

        Money minOrderAmount = !string.IsNullOrEmpty(request.CurrencyCode)
            ? Money.Create(request.MinOrderAmount, request.CurrencyCode)
            : Money.Zero(); 

        coupon.Update(
            request.Name,
            request.Code,
            request.StartDateTime,
            request.EndDateTime,
            request.EarlySaveDateTime,
            request.MaxUsageCount,
            discountAmount,
            request.DiscountPercentage,
            maxDiscountAmount,
            minOrderAmount,
            request.ApplicableProductIds
        );

        await _couponRepository.SaveChangesAsync(cancellationToken);

        var responseDto = coupon.ToDto();

        return responseDto;
    }
}

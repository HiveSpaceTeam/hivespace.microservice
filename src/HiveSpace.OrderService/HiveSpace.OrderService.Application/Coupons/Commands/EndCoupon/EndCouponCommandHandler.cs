using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Application.Coupons.Mappers;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Coupons.Commands.EndCoupon;

public class EndCouponCommandHandler(ICouponRepository couponRepository, IUserContext userContext)
    : ICommandHandler<EndCouponCommand, CouponDto>
{
    public async Task<CouponDto> Handle(EndCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await couponRepository.GetByIdAsync(request.Id, true, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CouponNotFound, request.Id.ToString());

        // Explicit Authorization Check
        if (userContext.IsSeller && userContext.StoreId.HasValue && 
            !new CouponOwnedByStoreSpecification(userContext.StoreId.Value).IsSatisfiedBy(coupon))
        {
             throw new ForbiddenException(OrderDomainErrorCode.CouponNotStoreOwned, request.Id.ToString());
        }

        coupon.End();

        await couponRepository.SaveChangesAsync(cancellationToken);

        return coupon.ToDto();
    }
}

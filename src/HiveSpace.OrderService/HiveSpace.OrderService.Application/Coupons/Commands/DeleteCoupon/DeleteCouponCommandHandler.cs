using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Specifications;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Coupons.Commands.DeleteCoupon;

public class DeleteCouponCommandHandler(ICouponRepository couponRepository, IUserContext userContext)
    : ICommandHandler<DeleteCouponCommand>
{
    public async Task Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the coupon by ID
        var coupon = await couponRepository.GetByIdAsync(request.Id, false, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CouponNotFound, request.Id.ToString());

        // 2. Explicit Authorization Check
        if (userContext.IsSeller && userContext.StoreId.HasValue && 
            !new CouponOwnedByStoreSpecification(userContext.StoreId.Value).IsSatisfiedBy(coupon))
        {
             throw new ForbiddenException(OrderDomainErrorCode.CouponNotStoreOwned, request.Id.ToString());
        }

        // 3. Status Check - Only upcoming coupons can be deleted
        var upcomingSpec = new CouponUpcomingSpecification();
        if (!upcomingSpec.IsSatisfiedBy(coupon))
        {
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidStatus, request.Id.ToString());
        }

        // 4. Remove and save
        couponRepository.Remove(coupon);
        await couponRepository.SaveChangesAsync(cancellationToken);
    }
}

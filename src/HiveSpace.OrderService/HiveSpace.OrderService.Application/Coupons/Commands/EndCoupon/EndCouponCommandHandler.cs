using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Application.Coupons.Mappers;

namespace HiveSpace.OrderService.Application.Coupons.Commands.EndCoupon;

public class EndCouponCommandHandler(ICouponRepository couponRepository, IUserContext userContext)
    : IRequestHandler<EndCouponCommand, CouponDto>
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

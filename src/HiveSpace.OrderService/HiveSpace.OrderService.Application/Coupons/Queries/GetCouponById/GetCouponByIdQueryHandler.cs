using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Application.Coupons.Mappers;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;

namespace HiveSpace.OrderService.Application.Coupons.Queries.GetCouponById;

public class GetCouponByIdQueryHandler(ICouponRepository couponRepository, IUserContext userContext)
    : IRequestHandler<GetCouponByIdQuery, CouponDto>
{
    public async Task<CouponDto> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch the coupon by ID
        var coupon = await couponRepository.GetByIdAsync(request.Id, true, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.CouponNotFound, request.Id.ToString());

        // 2. Explicit Authorization Check
        if (userContext.IsSeller && userContext.StoreId.HasValue && 
            !new CouponOwnedByStoreSpecification(userContext.StoreId.Value).IsSatisfiedBy(coupon))
        {
            throw new ForbiddenException(OrderDomainErrorCode.CouponNotStoreOwned, request.Id.ToString());
        }

        return coupon.ToDto();
    }
}

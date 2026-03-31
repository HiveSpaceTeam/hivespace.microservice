using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Models.Pagination;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Specifications;
using HiveSpace.OrderService.Application.Coupons.Dtos;
using HiveSpace.OrderService.Application.Coupons.Mappers;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Coupons.Specifications;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Repositories;

namespace HiveSpace.OrderService.Application.Coupons.Queries.GetCouponList;

public class GetCouponListQueryHandler(ICouponRepository couponRepository, IUserContext userContext)
    : IQueryHandler<GetCouponListQuery, GetCouponListResponse>
{
    public async Task<GetCouponListResponse> Handle(GetCouponListQuery request, CancellationToken cancellationToken)
    {
        // Build combined specification
        Specification<Coupon> specification = request.CouponStatus switch
        {
            CouponStatus.Ongoing => new CouponOngoingSpecification(),
            CouponStatus.Upcoming => new CouponUpcomingSpecification(),
            CouponStatus.Expired => new CouponExpiredSpecification(),
            _ => new TrueSpecification<Coupon>() // All
        };

        // Scope to the calling seller's store — admins/platform see all coupons
        if (userContext.IsSeller && userContext.StoreId.HasValue)
        {
            specification = specification.And(new CouponOwnedByStoreSpecification(userContext.StoreId.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.CouponName))
        {
            specification = specification.And(x => x.Name.Contains(request.CouponName));
        }

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            specification = specification.And(x => x.Code.Contains(request.CouponCode));
        }

        // Two efficient DB queries: count + paged fetch
        var totalItems = await couponRepository.GetCountAsync(specification, cancellationToken);
        var pagedCoupons = await couponRepository.GetPagedAsync(specification, request.Page, request.PageSize, cancellationToken);

        // Map to lean list DTOs
        var couponDtos = pagedCoupons.Select(coupon => coupon.ToSummaryDto()).ToList();

        var pagination = new PaginationMetadata(request.Page, request.PageSize, totalItems);

        return new GetCouponListResponse(couponDtos, pagination);
    }
}

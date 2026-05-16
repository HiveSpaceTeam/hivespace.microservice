using HiveSpace.Application.Shared.Queries;
namespace HiveSpace.OrderService.Application.Coupons.Queries.GetAvailableCoupons;

public record GetAvailableCouponsQuery(Guid StoreId, IReadOnlyCollection<long>? ProductIds = null)
    : IQuery<GetAvailableCouponsResponse>;

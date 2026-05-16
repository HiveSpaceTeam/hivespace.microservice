namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

public record StoreCouponSelectionDto(Guid StoreId, string CouponCode);

public record CheckoutCouponSelectionDto
{
    public List<string> PlatformCouponCodes { get; init; } = [];
    public List<StoreCouponSelectionDto> StoreCoupons { get; init; } = [];
}

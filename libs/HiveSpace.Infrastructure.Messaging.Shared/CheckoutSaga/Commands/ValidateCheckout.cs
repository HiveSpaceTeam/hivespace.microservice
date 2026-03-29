using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record ValidateCheckout
{
    public Guid               CorrelationId   { get; init; }
    public Guid               UserId          { get; init; }
    public List<string>       CouponCodes     { get; init; } = new();
    public DeliveryAddressDto DeliveryAddress { get; init; } = null!;
}

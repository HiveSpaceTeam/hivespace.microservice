using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record CheckoutInitiated
{
    public Guid               CorrelationId   { get; init; }
    public Guid               UserId          { get; init; }
    public DeliveryAddressDto DeliveryAddress { get; init; } = null!;
    public CheckoutCouponSelectionDto CouponSelections { get; init; } = new();
    public PaymentMethod      PaymentMethod   { get; init; } = PaymentMethod.COD;
}

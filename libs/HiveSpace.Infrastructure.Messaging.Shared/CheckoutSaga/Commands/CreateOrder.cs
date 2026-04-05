using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record CreateOrder
{
    public Guid               CorrelationId   { get; init; }
    public Guid               UserId          { get; init; }
    public DeliveryAddressDto DeliveryAddress { get; init; } = null!;
    public PaymentMethod      PaymentMethod   { get; init; } = PaymentMethod.COD;
    public List<string>       CouponCodes     { get; init; } = new();
}

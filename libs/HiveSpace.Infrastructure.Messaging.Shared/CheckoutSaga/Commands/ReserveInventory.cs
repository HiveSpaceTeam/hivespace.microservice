using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record ReserveInventory
{
    public Guid               CorrelationId     { get; init; }
    public List<Guid>         OrderIds          { get; init; } = new();
    public List<OrderItemDto> Items             { get; init; } = new();
    public int                ExpirationMinutes { get; init; } = 15;
}

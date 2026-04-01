namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record InventoryReservationFailed
{
    public Guid                      CorrelationId { get; init; }
    public List<Guid> OrderIds { get; init; } = new();
    public string                    Reason        { get; init; } = null!;
    public List<StockFailureDto>     Failures      { get; init; } = new();
}

public record StockFailureDto
{
    public long   SkuId             { get; init; }
    public int    RequestedQuantity { get; init; }
    public int    AvailableQuantity { get; init; }
    public string Reason            { get; init; } = null!;
}

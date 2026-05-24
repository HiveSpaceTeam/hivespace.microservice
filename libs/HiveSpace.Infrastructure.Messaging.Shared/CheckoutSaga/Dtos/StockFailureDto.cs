namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

public record StockFailureDto
{
    public long   SkuId             { get; init; }
    public int    RequestedQuantity { get; init; }
    public int    AvailableQuantity { get; init; }
    public string Reason            { get; init; } = null!;
}

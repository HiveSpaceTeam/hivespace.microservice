namespace HiveSpace.OrderService.Application.Contracts;

public record ClearCart
{
    public Guid CorrelationId { get; init; }
    public Guid UserId        { get; init; }
    public List<Guid> PurchasedStoreIds { get; init; } = [];
}

public record CartCleared
{
    public Guid CorrelationId { get; init; }
}

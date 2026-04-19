using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderCreated
{
    public Guid                        CorrelationId  { get; init; }
    public List<Guid>                  OrderIds       { get; init; } = new();
    public Dictionary<Guid, Guid>      OrderStoreMap  { get; init; } = new();   // OrderId → StoreId
    public long                        GrandTotal     { get; init; }
    public List<OrderItemDto>          Items          { get; init; } = new();
    public Dictionary<Guid, string>    OrderCodeMap   { get; init; } = new();   // OrderId → ShortId
    public DateTimeOffset              CreatedAt      { get; init; }
}

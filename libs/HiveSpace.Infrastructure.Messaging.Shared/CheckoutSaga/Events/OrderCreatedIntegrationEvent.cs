using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;

public record OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid                        CorrelationId  { get; init; }
    public List<Guid>                  OrderIds       { get; init; } = new();
    public Dictionary<Guid, Guid>      OrderStoreMap  { get; init; } = new();   // OrderId → StoreId
    public Dictionary<Guid, long>      OrderAmountMap { get; init; } = new();
    public long                        GrandTotal     { get; init; }
    public string                      CurrencyCode   { get; init; } = Currency.VND.GetCode();
    public List<OrderItemDto>          Items          { get; init; } = new();
    public Dictionary<Guid, string>    OrderCodeMap   { get; init; } = new();   // OrderId → OrderCode
    public DateTimeOffset              CreatedAt      { get; init; }
}

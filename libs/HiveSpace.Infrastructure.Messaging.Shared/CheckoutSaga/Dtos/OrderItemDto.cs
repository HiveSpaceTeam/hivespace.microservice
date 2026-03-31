namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

public record OrderItemDto
{
    public long    ProductId   { get; init; }
    public long    SkuId       { get; init; }
    public Guid    StoreId     { get; init; }
    public int     Quantity    { get; init; }
    public long    Price       { get; init; }
    public string  ProductName { get; init; } = null!;
    public string  SkuName     { get; init; } = null!;
    public string  ImageUrl    { get; init; } = null!;
}

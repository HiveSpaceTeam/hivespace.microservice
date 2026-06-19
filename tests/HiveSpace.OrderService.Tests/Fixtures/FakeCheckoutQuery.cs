using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Tests.Fixtures;

public sealed class FakeCheckoutQuery : ICheckoutQuery
{
    private readonly CheckoutPreviewRawRow[] _rows;

    public FakeCheckoutQuery(params CheckoutPreviewRawRow[] rows)
    {
        _rows = rows;
    }

    public Task<CheckoutPreviewRawResult> GetSelectedCartItemsAsync(
        Guid userId, CancellationToken ct = default)
        => Task.FromResult(new CheckoutPreviewRawResult(_rows, CartExists: _rows.Length > 0));

    public Task<CheckoutStatusDto> GetCheckoutStatusAsync(
        Guid correlationId, Guid userId, CancellationToken ct = default)
        => Task.FromResult(new CheckoutStatusDto
        {
            CorrelationId = correlationId,
            CurrentState  = "Pending"
        });

    public static CheckoutPreviewRawRow MakeRow(
        Guid storeId,
        long productId = 1L,
        long skuId     = 10L,
        int  quantity  = 1,
        long price     = 50_000)
        => new(
            CartItemId:   Guid.NewGuid(),
            ProductId:    productId,
            SkuId:        skuId,
            Quantity:     quantity,
            ProductName:  "Test Product",
            ThumbnailUrl: null,
            Price:        price,
            Currency:     "VND",
            SkuName:      "Test SKU",
            SkuImageUrl:  null,
            SkuAttributes:null,
            StoreId:      storeId,
            StoreName:    "Test Store");
}

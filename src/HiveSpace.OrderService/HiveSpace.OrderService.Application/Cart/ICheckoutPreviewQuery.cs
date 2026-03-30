using HiveSpace.OrderService.Application.Cart.Dtos;

namespace HiveSpace.OrderService.Application.Cart;

public interface ICheckoutPreviewQuery
{
    Task<CheckoutPreviewRawResult> GetSelectedCartItemsAsync(
        Guid userId, CancellationToken ct = default);
}

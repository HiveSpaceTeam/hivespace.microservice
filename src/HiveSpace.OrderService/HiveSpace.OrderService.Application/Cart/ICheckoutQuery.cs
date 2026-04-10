using HiveSpace.OrderService.Application.Cart.Dtos;
using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Application.Cart;

public interface ICheckoutQuery
{
    Task<CheckoutPreviewRawResult> GetSelectedCartItemsAsync(
        Guid userId, CancellationToken ct = default);

    Task<CheckoutStatusDto> GetCheckoutStatusAsync(
        Guid correlationId, Guid userId, CancellationToken ct = default);
}

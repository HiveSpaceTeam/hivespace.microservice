namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CheckoutPreviewRawResult(CheckoutPreviewRawRow[] Rows, bool CartExists);

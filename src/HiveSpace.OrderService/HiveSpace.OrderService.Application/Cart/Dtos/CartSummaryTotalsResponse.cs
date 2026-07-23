using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CartSummaryTotalsResponse(
    MoneyResponseDto DiscountAmount,
    MoneyResponseDto SubTotal,
    MoneyResponseDto Total);

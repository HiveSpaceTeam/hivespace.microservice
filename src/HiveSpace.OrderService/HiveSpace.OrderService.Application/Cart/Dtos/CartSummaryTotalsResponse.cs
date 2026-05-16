namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CartSummaryTotalsResponse(
    long DiscountAmount,
    long SubTotal,
    long Total);

namespace HiveSpace.PaymentService.Application.Payments.Dtos;

public record PaymentLinkedOrderDto(
    Guid OrderId,
    string OrderCode,
    Guid StoreId,
    PaymentMoneyDto Amount,
    string? StatusSnapshot);

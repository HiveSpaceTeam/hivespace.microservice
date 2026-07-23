using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.PaymentService.Application.Payments.Dtos;

public record PaymentLinkedOrderDto(
    Guid OrderId,
    string OrderCode,
    Guid StoreId,
    MoneyResponseDto Amount,
    string? StatusSnapshot);

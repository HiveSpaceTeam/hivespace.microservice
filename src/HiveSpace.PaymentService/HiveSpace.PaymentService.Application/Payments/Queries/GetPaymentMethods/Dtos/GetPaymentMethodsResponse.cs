namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentMethods.Dtos;

public record GetPaymentMethodsResponse(IReadOnlyList<PaymentMethodMetadataDto> Methods);

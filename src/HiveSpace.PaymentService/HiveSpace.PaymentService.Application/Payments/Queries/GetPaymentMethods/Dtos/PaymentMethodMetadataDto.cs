namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentMethods.Dtos;

public record PaymentMethodMetadataDto(
    string Code,
    string DisplayName,
    string Kind,
    string? GatewayCode,
    bool IsEnabled,
    bool IsCheckoutSelectable,
    string Availability,
    int SortOrder);

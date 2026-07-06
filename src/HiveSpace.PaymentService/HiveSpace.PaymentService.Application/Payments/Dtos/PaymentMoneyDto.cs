namespace HiveSpace.PaymentService.Application.Payments.Dtos;

public record PaymentMoneyDto(long Amount, string? CurrencyCode, bool IsValid = true, string? IssueCode = null);

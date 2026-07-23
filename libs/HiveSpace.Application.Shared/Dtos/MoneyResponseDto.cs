namespace HiveSpace.Application.Shared.Dtos;

public record MoneyResponseDto(
    long Amount,
    string? CurrencyCode,
    bool IsValid = true,
    string? IssueCode = null,
    string? DisplayPlaceholder = null)
{
    public static MoneyResponseDto Valid(long amount, string currencyCode)
        => new(amount, currencyCode, true, null, null);

    public static MoneyResponseDto Invalid(
        long amount = 0,
        string? currencyCode = null,
        string issueCode = "invalid_money",
        string? displayPlaceholder = null)
        => new(amount, currencyCode, false, issueCode, displayPlaceholder);
}

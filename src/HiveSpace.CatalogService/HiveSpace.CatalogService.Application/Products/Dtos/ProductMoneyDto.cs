namespace HiveSpace.CatalogService.Application.Products.Dtos;

public record ProductMoneyDto(long Amount, string? CurrencyCode, bool IsValid = true, string? IssueCode = null);

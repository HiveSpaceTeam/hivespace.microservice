namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedPriceRequestDto(
    long? Amount,
    string? CurrencyCode,
    string? SourceRawValue);

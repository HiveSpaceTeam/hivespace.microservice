namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedSellerRequestDto(
    string ExternalSellerId,
    string DisplayName,
    string? Slug,
    string? Url,
    IReadOnlyDictionary<string, object>? Metadata,
    string LogoUrl);

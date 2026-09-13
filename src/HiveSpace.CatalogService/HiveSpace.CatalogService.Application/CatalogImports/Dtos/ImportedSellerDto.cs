namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportedSellerDto(
    Guid ImportedSellerId,
    string ExternalSellerId,
    string DisplayName,
    string? Slug,
    string? Url,
    string LogoUrl,
    string Status,
    Guid? UserId,
    Guid? StoreId,
    string? ConflictReason,
    IReadOnlyCollection<ExistingStoreCandidateDto> ExistingStoreCandidates,
    bool RequiresValidationRerun);

public record ExistingStoreCandidateDto(
    Guid UserId,
    Guid StoreId,
    string StoreName,
    string? Reason);

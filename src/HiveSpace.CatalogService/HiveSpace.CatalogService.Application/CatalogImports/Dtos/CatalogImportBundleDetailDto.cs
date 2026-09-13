namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportBundleDetailDto(
    CatalogImportBundleSummaryDto Bundle,
    IReadOnlyCollection<ImportedSellerDto> Sellers,
    IReadOnlyCollection<SellerOwnershipLinkDto> SellerOwnershipCandidates,
    IReadOnlyCollection<ImportedCategoryMappingDto> CategoryLinks,
    IReadOnlyCollection<ImportedProductDto> Products,
    IReadOnlyCollection<ImportDuplicateGroupDto> DuplicateGroups,
    IReadOnlyCollection<ImportValidationIssueDto> ValidationIssues);

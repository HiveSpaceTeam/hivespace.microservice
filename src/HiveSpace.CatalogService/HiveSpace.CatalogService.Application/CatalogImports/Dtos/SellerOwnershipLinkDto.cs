namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record SellerOwnershipLinkDto(
    Guid Id,
    Guid ImportedSellerId,
    string SourceSystem,
    string ExternalSellerId,
    Guid? HiveSpaceUserId,
    Guid? HiveSpaceStoreId,
    string LinkStatus,
    Guid CreatedByUserId,
    Guid? ApprovedByUserId,
    DateTimeOffset? ApprovedAt,
    string? ApprovalReason);

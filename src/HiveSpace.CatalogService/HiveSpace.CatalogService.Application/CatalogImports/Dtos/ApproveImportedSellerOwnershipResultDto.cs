namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ApproveImportedSellerOwnershipResultDto(
    Guid ImportedSellerId,
    string ExternalSellerId,
    Guid? UserId,
    Guid? StoreId,
    string Status,
    string? ConflictReason);

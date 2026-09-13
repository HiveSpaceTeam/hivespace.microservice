namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ApproveImportedSellerOwnershipRequestDto(
    Guid TargetUserId,
    Guid TargetStoreId,
    string ApprovalReason);

using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ApproveImportedSellerOwnership;

public record ApproveImportedSellerOwnershipCommand(
    Guid BundleId,
    Guid ImportedSellerId,
    Guid TargetUserId,
    Guid TargetStoreId,
    Guid ApprovedByUserId,
    string ApprovalReason) : ICommand<ApproveImportedSellerOwnershipResultDto>;

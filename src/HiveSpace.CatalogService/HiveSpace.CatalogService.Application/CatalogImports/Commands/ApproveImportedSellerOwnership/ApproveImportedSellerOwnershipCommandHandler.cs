using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ApproveImportedSellerOwnership;

public class ApproveImportedSellerOwnershipCommandHandler(
    ICatalogImportBundleRepository repository,
    IStoreRefRepository storeRefRepository)
    : ICommandHandler<ApproveImportedSellerOwnershipCommand, ApproveImportedSellerOwnershipResultDto>
{
    public async Task<ApproveImportedSellerOwnershipResultDto> Handle(
        ApproveImportedSellerOwnershipCommand request,
        CancellationToken cancellationToken)
    {
        var bundle = await repository.GetByIdAsync(request.BundleId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportBundleNotFound, nameof(CatalogImportBundle));
        var seller = bundle.Sellers.FirstOrDefault(x => x.Id == request.ImportedSellerId)
            ?? throw new NotFoundException(CatalogDomainErrorCode.InvalidImportedSeller, nameof(ImportedSeller));
        if (seller.ProvisioningStatus != Domain.CatalogImports.Enums.ImportedSellerProvisioningStatus.Conflict)
        {
            return new ApproveImportedSellerOwnershipResultDto(
                seller.Id,
                seller.ExternalSellerId,
                seller.HiveSpaceUserId,
                seller.HiveSpaceStoreId,
                "Conflict",
                "ImportedSellerDoesNotRequireApproval");
        }

        var targetStore = await storeRefRepository.GetByIdAsync(request.TargetStoreId, cancellationToken);
        if (!IsEligibleTarget(seller, targetStore, request.TargetUserId))
        {
            return new ApproveImportedSellerOwnershipResultDto(
                seller.Id,
                seller.ExternalSellerId,
                seller.HiveSpaceUserId,
                seller.HiveSpaceStoreId,
                "Conflict",
                "IneligibleSellerOwnershipTarget");
        }

        bundle.ApproveSellerOwnership(
            seller.Id,
            request.TargetUserId,
            request.TargetStoreId,
            request.ApprovedByUserId,
            request.ApprovalReason);

        bundle.RefreshSummary();
        await repository.SaveChangesAsync(cancellationToken);

        return new ApproveImportedSellerOwnershipResultDto(
            seller.Id,
            seller.ExternalSellerId,
            seller.HiveSpaceUserId,
            seller.HiveSpaceStoreId,
            seller.ProvisioningStatus.ToString(),
            seller.ConflictReason);
    }

    private static bool IsEligibleTarget(ImportedSeller seller, StoreRef? storeRef, Guid targetUserId)
    {
        if (storeRef is null || storeRef.OwnerId != targetUserId)
            return false;

        if (seller.SuggestedHiveSpaceStoreId.HasValue || seller.SuggestedHiveSpaceUserId.HasValue)
        {
            return seller.SuggestedHiveSpaceStoreId == storeRef.Id
                && seller.SuggestedHiveSpaceUserId == targetUserId;
        }

        return true;
    }
}

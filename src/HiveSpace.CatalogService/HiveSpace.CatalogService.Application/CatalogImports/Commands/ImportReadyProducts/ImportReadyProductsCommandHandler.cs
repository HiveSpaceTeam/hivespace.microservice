using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using System.Text.Json;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ImportReadyProducts;

public class ImportReadyProductsCommandHandler(
    ICatalogImportBundleRepository importRepository,
    ICatalogImportJobScheduler jobScheduler)
    : ICommandHandler<ImportReadyProductsCommand, CatalogImportJobSubmissionDto>
{
    public async Task<CatalogImportJobSubmissionDto> Handle(
        ImportReadyProductsCommand request,
        CancellationToken cancellationToken)
    {
        var bundle = await importRepository.GetByIdAsync(request.BundleId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportBundleNotFound, nameof(CatalogImportBundle));

        var existingJob = await importRepository.GetActiveJobByOperationAndSourceFingerprintAsync(
            CatalogImportJobOperationType.ImportReadyProducts,
            bundle.SourceSystem,
            bundle.SourceFingerprint,
            cancellationToken);
        if (existingJob is not null)
            return CatalogImportJobMapper.ToSubmissionDto(existingJob);

        var importAllEligible = request.ProductIds is null || request.ProductIds.Count == 0;
        var acceptedProductIds = importAllEligible
            ? bundle.Products
                .Where(product => CanImport(bundle, product))
                .Select(product => product.Id)
                .ToList()
            : request.ProductIds!
                .Distinct()
                .ToList();

        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ImportReadyProducts,
            bundle.SourceSystem,
            request.ImportedByUserId,
            sourceFingerprint: bundle.SourceFingerprint,
            sourceFileName: bundle.SourceFileName,
            bundleId: bundle.Id,
            requestPayloadJson: JsonSerializer.Serialize(new ImportReadyProductsJobPayload(
                acceptedProductIds,
                request.PublicationState,
                request.ImportedByUserId,
                importAllEligible,
                request.ProductIds)));

        importRepository.AddJob(job);
        await jobScheduler.ScheduleAsync(job, cancellationToken);
        await importRepository.SaveChangesAsync(cancellationToken);

        return CatalogImportJobMapper.ToSubmissionDto(job);
    }

    private static bool CanImport(CatalogImportBundle bundle, ImportedProduct product)
    {
        if (product.ImportStatus != ImportedProductImportStatus.NotImported)
            return false;
        if (product.ReadinessStatus is ImportedProductReadinessStatus.Blocked or ImportedProductReadinessStatus.Duplicate or ImportedProductReadinessStatus.Imported)
            return false;

        var seller = bundle.Sellers.FirstOrDefault(x =>
            string.Equals(x.ExternalSellerId, product.ExternalSellerId, StringComparison.OrdinalIgnoreCase));
        if (seller?.HiveSpaceStoreId is null)
            return false;

        if (product.ExternalCategoryIds.Count == 0
            || product.ExternalCategoryIds.Any(categoryId =>
                !bundle.CategoryMappings.Any(mapping =>
                    string.Equals(mapping.ExternalCategoryId, categoryId, StringComparison.OrdinalIgnoreCase)
                    && mapping.HiveSpaceCategoryId.HasValue)))
            return false;

        if (!product.Skus.Any(sku =>
                sku.PriceAmount is > 0
                && string.Equals(sku.CurrencyCode, "VND", StringComparison.OrdinalIgnoreCase)))
            return false;

        if (bundle.ValidationIssues.Any(issue =>
                issue.Severity == ImportValidationSeverity.Blocking
                && (string.Equals(issue.EntitySourceId, product.ExternalProductId, StringComparison.OrdinalIgnoreCase)
                    || product.Skus.Any(sku => string.Equals(issue.EntitySourceId, sku.ExternalSkuId, StringComparison.OrdinalIgnoreCase)))))
            return false;

        return bundle.DuplicateGroups.All(group =>
            !group.MemberImportedProductIds.Contains(product.Id)
            || group.RepresentativeImportedProductId == product.Id);
    }
}

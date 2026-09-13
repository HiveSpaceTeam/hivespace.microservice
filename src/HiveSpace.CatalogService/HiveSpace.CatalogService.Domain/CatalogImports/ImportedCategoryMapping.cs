using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ImportedCategoryMapping
{
    public Guid Id { get; private set; }
    public Guid BundleId { get; private set; }
    public string ExternalCategoryId { get; private set; } = string.Empty;
    public string? ExternalParentCategoryId { get; private set; }
    public string ExternalCategoryName { get; private set; } = string.Empty;
    public string? PathJson { get; private set; }
    public int? HiveSpaceCategoryId { get; private set; }
    public ImportedCategoryMappingStatus MappingStatus { get; private set; }
    public Guid? MappedByUserId { get; private set; }
    public DateTimeOffset? MappedAt { get; private set; }

    private ImportedCategoryMapping()
    {
    }

    public static ImportedCategoryMapping Create(
        Guid bundleId,
        string externalCategoryId,
        string? externalParentCategoryId,
        string externalCategoryName,
        string? pathJson)
    {
        if (string.IsNullOrWhiteSpace(externalCategoryId))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(ExternalCategoryId));
        if (string.IsNullOrWhiteSpace(externalCategoryName))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(ExternalCategoryName));

        return new ImportedCategoryMapping
        {
            Id = Guid.NewGuid(),
            BundleId = bundleId,
            ExternalCategoryId = externalCategoryId.Trim(),
            ExternalParentCategoryId = externalParentCategoryId,
            ExternalCategoryName = externalCategoryName.Trim(),
            PathJson = pathJson,
            MappingStatus = ImportedCategoryMappingStatus.Unmapped
        };
    }

    public void MapToCategory(int hiveSpaceCategoryId, Guid mappedByUserId)
    {
        if (hiveSpaceCategoryId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(HiveSpaceCategoryId));
        if (mappedByUserId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(MappedByUserId));

        HiveSpaceCategoryId = hiveSpaceCategoryId;
        MappedByUserId = mappedByUserId;
        MappedAt = DateTimeOffset.UtcNow;
        MappingStatus = ImportedCategoryMappingStatus.Mapped;
    }
}

using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ImportedImageReference
{
    public Guid Id { get; private set; }
    public Guid ImportedProductId { get; private set; }
    public Guid? ImportedSkuId { get; private set; }
    public string ExternalUrl { get; private set; } = string.Empty;
    public string? SourceImageId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public ImportedImageMediaStatus MediaStatus { get; private set; }
    public string? MediaFileId { get; private set; }

    private ImportedImageReference()
    {
    }

    public static ImportedImageReference Create(Guid importedProductId, Guid? importedSkuId, string externalUrl, string role, string? sourceImageId)
    {
        if (string.IsNullOrWhiteSpace(externalUrl))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedImage, nameof(ExternalUrl));
        if (string.IsNullOrWhiteSpace(role))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedImage, nameof(Role));

        return new ImportedImageReference
        {
            Id = Guid.NewGuid(),
            ImportedProductId = importedProductId,
            ImportedSkuId = importedSkuId,
            ExternalUrl = externalUrl.Trim(),
            Role = role.Trim(),
            SourceImageId = sourceImageId,
            MediaStatus = ImportedImageMediaStatus.ExternalOnly
        };
    }
}

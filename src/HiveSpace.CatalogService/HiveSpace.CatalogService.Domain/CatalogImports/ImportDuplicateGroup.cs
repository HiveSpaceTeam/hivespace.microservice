using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using System.Text.Json;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ImportDuplicateGroup
{
    public Guid Id { get; private set; }
    public Guid BundleId { get; private set; }
    public string DuplicateKey { get; private set; } = string.Empty;
    public string ExternalProductIdsJson { get; private set; } = "[]";
    public string MemberImportedProductIdsJson { get; private set; } = "[]";
    public Guid? RepresentativeImportedProductId { get; private set; }
    public ImportDuplicateGroupResolutionStatus ResolutionStatus { get; private set; }
    public IReadOnlyCollection<Guid> MemberImportedProductIds
        => JsonSerializer.Deserialize<IReadOnlyCollection<Guid>>(MemberImportedProductIdsJson) ?? [];

    private ImportDuplicateGroup()
    {
    }

    public static ImportDuplicateGroup Create(
        Guid bundleId,
        string duplicateKey,
        IReadOnlyCollection<string> externalProductIds,
        IReadOnlyCollection<Guid> memberImportedProductIds)
    {
        if (string.IsNullOrWhiteSpace(duplicateKey))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(DuplicateKey));
        if (externalProductIds.Count < 2)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(ExternalProductIdsJson));
        if (memberImportedProductIds.Count < 2)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(MemberImportedProductIdsJson));

        return new ImportDuplicateGroup
        {
            Id = Guid.NewGuid(),
            BundleId = bundleId,
            DuplicateKey = duplicateKey.Trim(),
            ExternalProductIdsJson = JsonSerializer.Serialize(externalProductIds),
            MemberImportedProductIdsJson = JsonSerializer.Serialize(memberImportedProductIds),
            ResolutionStatus = ImportDuplicateGroupResolutionStatus.Unresolved
        };
    }

    public void Resolve(Guid representativeImportedProductId)
    {
        if (!MemberImportedProductIds.Contains(representativeImportedProductId))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(RepresentativeImportedProductId));

        RepresentativeImportedProductId = representativeImportedProductId;
        ResolutionStatus = ImportDuplicateGroupResolutionStatus.Resolved;
    }
}

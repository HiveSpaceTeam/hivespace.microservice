using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using System.Text.Json;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ImportedAttribute
{
    public Guid Id { get; private set; }
    public Guid ImportedProductId { get; private set; }
    public string ExternalAttributeName { get; private set; } = string.Empty;
    public string? ExternalAttributeValue { get; private set; }
    public string? SourceAttributeId { get; private set; }
    public string? SourceValueId { get; private set; }
    public int? HiveSpaceAttributeDefinitionId { get; private set; }
    public string MatchedAttributeValueIdsJson { get; private set; } = "[]";
    public ImportedAttributeMatchStatus MatchStatus { get; private set; }
    public IReadOnlyCollection<int> MatchedAttributeValueIds
        => JsonSerializer.Deserialize<IReadOnlyCollection<int>>(MatchedAttributeValueIdsJson) ?? [];

    private ImportedAttribute()
    {
    }

    public static ImportedAttribute Create(Guid importedProductId, string name, string? value, string? sourceAttributeId, string? sourceValueId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, nameof(ExternalAttributeName));

        return new ImportedAttribute
        {
            Id = Guid.NewGuid(),
            ImportedProductId = importedProductId,
            ExternalAttributeName = name.Trim(),
            ExternalAttributeValue = value,
            SourceAttributeId = sourceAttributeId,
            SourceValueId = sourceValueId,
            MatchStatus = ImportedAttributeMatchStatus.Unmatched
        };
    }

    public void MatchToDefinition(int hiveSpaceAttributeDefinitionId, IEnumerable<int>? matchedAttributeValueIds, string? freeTextValue)
    {
        if (hiveSpaceAttributeDefinitionId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, nameof(HiveSpaceAttributeDefinitionId));

        HiveSpaceAttributeDefinitionId = hiveSpaceAttributeDefinitionId;
        MatchedAttributeValueIdsJson = JsonSerializer.Serialize((matchedAttributeValueIds ?? []).Distinct().ToArray());
        ExternalAttributeValue = string.IsNullOrWhiteSpace(freeTextValue) ? null : freeTextValue.Trim();
        MatchStatus = ImportedAttributeMatchStatus.Matched;
    }

    public void MarkConflict()
    {
        MatchStatus = ImportedAttributeMatchStatus.Conflict;
    }
}

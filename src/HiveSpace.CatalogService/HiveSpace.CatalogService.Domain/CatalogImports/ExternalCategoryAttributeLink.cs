using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class ExternalCategoryAttributeLink
{
    public Guid Id { get; private set; }
    public string SourceSystem { get; private set; } = string.Empty;
    public string ExternalCategoryId { get; private set; } = string.Empty;
    public string SourceAttributeId { get; private set; } = string.Empty;
    public string ExternalAttributeName { get; private set; } = string.Empty;
    public string InputType { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }
    public int HiveSpaceCategoryId { get; private set; }
    public int HiveSpaceAttributeDefinitionId { get; private set; }
    public string SelectableValuesJson { get; private set; } = "[]";
    public string SourceFingerprint { get; private set; } = string.Empty;
    public Guid ProvisionedByUserId { get; private set; }
    public DateTimeOffset ProvisionedAt { get; private set; }

    [NotMapped]
    public IReadOnlyCollection<ExternalCategoryAttributeValueLink> SelectableValues
        => JsonSerializer.Deserialize<IReadOnlyCollection<ExternalCategoryAttributeValueLink>>(SelectableValuesJson) ?? [];

    private ExternalCategoryAttributeLink()
    {
    }

    public static ExternalCategoryAttributeLink Create(
        string sourceSystem,
        string externalCategoryId,
        string sourceAttributeId,
        string externalAttributeName,
        string inputType,
        bool isRequired,
        int hiveSpaceCategoryId,
        int hiveSpaceAttributeDefinitionId,
        IReadOnlyCollection<ExternalCategoryAttributeValueLink> selectableValues,
        string sourceFingerprint,
        Guid provisionedByUserId)
    {
        Require(sourceSystem, nameof(SourceSystem));
        Require(externalCategoryId, nameof(ExternalCategoryId));
        Require(sourceAttributeId, nameof(SourceAttributeId));
        Require(externalAttributeName, nameof(ExternalAttributeName));
        Require(inputType, nameof(InputType));
        Require(sourceFingerprint, nameof(SourceFingerprint));
        if (hiveSpaceCategoryId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, nameof(HiveSpaceCategoryId));
        if (hiveSpaceAttributeDefinitionId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, nameof(HiveSpaceAttributeDefinitionId));
        if (provisionedByUserId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, nameof(ProvisionedByUserId));

        return new ExternalCategoryAttributeLink
        {
            Id = Guid.NewGuid(),
            SourceSystem = sourceSystem.Trim(),
            ExternalCategoryId = externalCategoryId.Trim(),
            SourceAttributeId = sourceAttributeId.Trim(),
            ExternalAttributeName = externalAttributeName.Trim(),
            InputType = inputType.Trim(),
            IsRequired = isRequired,
            HiveSpaceCategoryId = hiveSpaceCategoryId,
            HiveSpaceAttributeDefinitionId = hiveSpaceAttributeDefinitionId,
            SelectableValuesJson = JsonSerializer.Serialize(selectableValues),
            SourceFingerprint = sourceFingerprint.Trim(),
            ProvisionedByUserId = provisionedByUserId,
            ProvisionedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        string externalAttributeName,
        string inputType,
        bool isRequired,
        int hiveSpaceCategoryId,
        int hiveSpaceAttributeDefinitionId,
        IReadOnlyCollection<ExternalCategoryAttributeValueLink> selectableValues,
        string sourceFingerprint,
        Guid provisionedByUserId)
    {
        Require(externalAttributeName, nameof(ExternalAttributeName));
        Require(inputType, nameof(InputType));
        Require(sourceFingerprint, nameof(SourceFingerprint));
        if (hiveSpaceCategoryId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, nameof(HiveSpaceCategoryId));
        if (hiveSpaceAttributeDefinitionId <= 0)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, nameof(HiveSpaceAttributeDefinitionId));
        if (provisionedByUserId == Guid.Empty)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, nameof(ProvisionedByUserId));

        ExternalAttributeName = externalAttributeName.Trim();
        InputType = inputType.Trim();
        IsRequired = isRequired;
        HiveSpaceCategoryId = hiveSpaceCategoryId;
        HiveSpaceAttributeDefinitionId = hiveSpaceAttributeDefinitionId;
        SelectableValuesJson = JsonSerializer.Serialize(selectableValues);
        SourceFingerprint = sourceFingerprint.Trim();
        ProvisionedByUserId = provisionedByUserId;
        ProvisionedAt = DateTimeOffset.UtcNow;
    }

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedAttribute, field);
    }
}

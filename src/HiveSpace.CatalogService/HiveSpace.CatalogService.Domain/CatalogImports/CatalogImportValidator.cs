using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using System.Text.Json;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public class CatalogImportValidator
{
    public IReadOnlyCollection<ImportValidationIssue> ValidateBundle(
        CatalogImportBundle bundle,
        bool vndEnabled,
        IReadOnlyCollection<ExternalCategoryAttributeLink>? attributeLinks = null)
        => Validate(bundle, vndEnabled, attributeLinks);

    public static IReadOnlyCollection<ImportValidationIssue> Validate(
        CatalogImportBundle bundle,
        bool vndEnabled,
        IReadOnlyCollection<ExternalCategoryAttributeLink>? attributeLinks = null)
    {
        var issues = new List<ImportValidationIssue>();
        var mappedCategories = bundle.CategoryMappings.ToDictionary(x => x.ExternalCategoryId, StringComparer.OrdinalIgnoreCase);
        var sellers = bundle.Sellers.ToDictionary(x => x.ExternalSellerId, StringComparer.OrdinalIgnoreCase);
        var attributeLinksByCategory = (attributeLinks ?? [])
            .GroupBy(x => x.ExternalCategoryId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var category in bundle.CategoryMappings.Where(x => x.HiveSpaceCategoryId is null))
        {
            issues.Add(CreateIssue(bundle, "Category", category.ExternalCategoryId, "hiveSpaceCategoryId", ImportValidationSeverity.Blocking, "UnprovisionedCategory", $"Imported category '{category.ExternalCategoryName}' must be provisioned before import."));
        }

        if (!vndEnabled)
        {
            issues.Add(CreateIssue(bundle, "Bundle", bundle.SourceFingerprint, "currencyPolicy", ImportValidationSeverity.Blocking, "VndCurrencyDisabled", "VND must be enabled before Tiki products can be imported."));
        }

        foreach (var product in bundle.Products)
        {
            ValidateProduct(bundle, product, sellers, mappedCategories, attributeLinksByCategory, vndEnabled, issues);
        }

        foreach (var group in bundle.DuplicateGroups.Where(x => x.ResolutionStatus == ImportDuplicateGroupResolutionStatus.Unresolved))
        {
            foreach (var product in bundle.Products.Where(product => group.MemberImportedProductIds.Contains(product.Id)))
            {
                issues.Add(CreateIssue(bundle, "Product", product.ExternalProductId, "duplicateGroup", ImportValidationSeverity.Blocking, "UnresolvedDuplicate", "Duplicate imported products must be resolved before import."));
            }
        }

        return issues;
    }

    private static void ValidateProduct(
        CatalogImportBundle bundle,
        ImportedProduct product,
        IReadOnlyDictionary<string, ImportedSeller> sellers,
        IReadOnlyDictionary<string, ImportedCategoryMapping> mappedCategories,
        IReadOnlyDictionary<string, List<ExternalCategoryAttributeLink>> attributeLinksByCategory,
        bool vndEnabled,
        List<ImportValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(product.Title))
            issues.Add(CreateIssue(bundle, "Product", product.ExternalProductId, "title", ImportValidationSeverity.Blocking, "MissingProductTitle", "Product title is required."));

        if (!sellers.TryGetValue(product.ExternalSellerId, out var seller) || !seller.HasActiveOwnership)
            issues.Add(CreateIssue(bundle, "Product", product.ExternalProductId, "sellerOwnership", ImportValidationSeverity.Blocking, "MissingSellerOwnership", "Seller account and store ownership must exist before import."));
        else if (seller.ProvisioningStatus is ImportedSellerProvisioningStatus.Conflict or ImportedSellerProvisioningStatus.Failed)
            issues.Add(CreateIssue(bundle, "Product", product.ExternalProductId, "sellerOwnership", ImportValidationSeverity.Blocking, seller.ConflictReason ?? "SellerOwnershipConflict", "Seller ownership conflict blocks import."));

        var missingExternalCategoryIds = product.ExternalCategoryIds
            .Where(id => !mappedCategories.TryGetValue(id, out var mapping) || mapping.HiveSpaceCategoryId is null)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (product.ExternalCategoryIds.Count == 0 || missingExternalCategoryIds.Count > 0)
            issues.Add(CreateIssue(
                bundle,
                "Product",
                product.ExternalProductId,
                "category",
                ImportValidationSeverity.Blocking,
                "UnprovisionedCategory",
                "Every imported product category must resolve to a provisioned HiveSpace category link.",
                missingExternalCategoryIds.Count == 0 ? null : JsonSerializer.Serialize(new { missingExternalCategoryIds })));

        if (!HasUsableProductMedia(product))
            issues.Add(CreateIssue(bundle, "Product", product.ExternalProductId, "images", ImportValidationSeverity.Blocking, "MissingUsableProductMedia", "At least one usable product image or thumbnail is required before import."));

        ValidateAttributes(bundle, product, attributeLinksByCategory, issues);

        if (product.Skus.Count == 0)
        {
            issues.Add(CreateIssue(bundle, "Product", product.ExternalProductId, "skus", ImportValidationSeverity.Blocking, "MissingSku", "At least one SKU is required."));
            return;
        }

        foreach (var sku in product.Skus)
        {
            if (sku.PriceAmount is null or <= 0 || !string.Equals(sku.CurrencyCode, "VND", StringComparison.OrdinalIgnoreCase))
                issues.Add(CreateIssue(bundle, "Sku", sku.ExternalSkuId, "price", ImportValidationSeverity.Blocking, "InvalidVndPrice", "SKU price must be positive VND."));

            if (!vndEnabled)
                issues.Add(CreateIssue(bundle, "Sku", sku.ExternalSkuId, "currencyCode", ImportValidationSeverity.Blocking, "VndCurrencyDisabled", "VND is not enabled in the local currency policy projection."));

            if (sku.StockQuantity is null)
                issues.Add(CreateIssue(bundle, "Sku", sku.ExternalSkuId, "stockQuantity", ImportValidationSeverity.Warning, "UnknownStockQuantity", "Stock quantity is unknown."));
        }
    }

    private static bool HasUsableProductMedia(ImportedProduct product)
    {
        if (!string.IsNullOrWhiteSpace(product.ThumbnailImageExternalUrl))
            return true;

        return product.Images.Any(image =>
            image.ImportedSkuId is null
            && image.MediaStatus is not (ImportedImageMediaStatus.Unsupported or ImportedImageMediaStatus.Inaccessible)
            && !string.IsNullOrWhiteSpace(image.ExternalUrl));
    }

    private static void ValidateAttributes(
        CatalogImportBundle bundle,
        ImportedProduct product,
        IReadOnlyDictionary<string, List<ExternalCategoryAttributeLink>> attributeLinksByCategory,
        List<ImportValidationIssue> issues)
    {
        var applicableLinks = product.ExternalCategoryIds
            .Where(attributeLinksByCategory.ContainsKey)
            .SelectMany(categoryId => attributeLinksByCategory[categoryId])
            .ToList();
        if (applicableLinks.Count == 0)
            return;

        foreach (var importedAttribute in product.Attributes)
        {
            var link = applicableLinks.FirstOrDefault(x =>
                           !string.IsNullOrWhiteSpace(importedAttribute.SourceAttributeId)
                           && string.Equals(x.SourceAttributeId, importedAttribute.SourceAttributeId, StringComparison.OrdinalIgnoreCase))
                       ?? applicableLinks.FirstOrDefault(x =>
                           string.Equals(x.ExternalAttributeName, importedAttribute.ExternalAttributeName, StringComparison.OrdinalIgnoreCase));

            if (link is null)
            {
                importedAttribute.MarkConflict();
                issues.Add(CreateIssue(
                    bundle,
                    "Product",
                    product.ExternalProductId,
                    "attributes",
                    ImportValidationSeverity.Warning,
                    "UnmatchedOptionalAttribute",
                    $"Imported attribute '{importedAttribute.ExternalAttributeName}' has no provisioned category attribute mapping and remains reviewable."));
                continue;
            }

            if (link.SelectableValues.Count > 0)
            {
                var matchedValue = !string.IsNullOrWhiteSpace(importedAttribute.SourceValueId)
                    ? link.SelectableValues.FirstOrDefault(x =>
                        string.Equals(x.SourceValueId, importedAttribute.SourceValueId, StringComparison.OrdinalIgnoreCase))
                    : null;

                if (matchedValue is not null)
                {
                    importedAttribute.MatchToDefinition(link.HiveSpaceAttributeDefinitionId, [matchedValue.HiveSpaceAttributeValueId], null);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(importedAttribute.ExternalAttributeValue))
                {
                    importedAttribute.MatchToDefinition(link.HiveSpaceAttributeDefinitionId, [], importedAttribute.ExternalAttributeValue);
                    issues.Add(CreateIssue(
                        bundle,
                        "Product",
                        product.ExternalProductId,
                        "attributes",
                        ImportValidationSeverity.Warning,
                        "AttributeValueFreeTextFallback",
                        $"Imported attribute '{importedAttribute.ExternalAttributeName}' used free-text fallback because no selectable source value match exists."));
                    continue;
                }

                importedAttribute.MarkConflict();
                issues.Add(CreateIssue(
                    bundle,
                    "Product",
                    product.ExternalProductId,
                    "attributes",
                    link.IsRequired ? ImportValidationSeverity.Blocking : ImportValidationSeverity.Warning,
                    "MissingSelectableAttributeValue",
                    $"Imported attribute '{importedAttribute.ExternalAttributeName}' is missing a selectable value match."));
                continue;
            }

            importedAttribute.MatchToDefinition(link.HiveSpaceAttributeDefinitionId, [], importedAttribute.ExternalAttributeValue);
        }

        foreach (var requiredLink in applicableLinks.Where(x => x.IsRequired))
        {
            var hasRequiredAttribute = product.Attributes.Any(x =>
                string.Equals(x.SourceAttributeId, requiredLink.SourceAttributeId, StringComparison.OrdinalIgnoreCase)
                || (x.HiveSpaceAttributeDefinitionId.HasValue && x.HiveSpaceAttributeDefinitionId.Value == requiredLink.HiveSpaceAttributeDefinitionId));

            if (!hasRequiredAttribute)
            {
                issues.Add(CreateIssue(
                    bundle,
                    "Product",
                    product.ExternalProductId,
                    "attributes",
                    ImportValidationSeverity.Blocking,
                    "MissingRequiredCategoryAttribute",
                    $"Required category attribute '{requiredLink.ExternalAttributeName}' is missing."));
            }
        }
    }

    private static ImportValidationIssue CreateIssue(
        CatalogImportBundle bundle,
        string entityType,
        string entitySourceId,
        string? field,
        ImportValidationSeverity severity,
        string reasonCode,
        string message,
        string? metadataJson = null)
        => ImportValidationIssue.Create(bundle.Id, entityType, entitySourceId, field, severity, reasonCode, message, metadataJson);
}

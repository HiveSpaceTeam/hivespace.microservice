using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Application.CatalogImports.Mappers;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Queries.ListCatalogImportBundleSection;

public class ListCatalogImportBundleSectionQueryHandler(ICatalogImportBundleRepository repository)
    : IQueryHandler<ListCatalogImportBundleSectionQuery, object>
{
    public async Task<object> Handle(ListCatalogImportBundleSectionQuery request, CancellationToken cancellationToken)
    {
        var bundle = await repository.GetByIdAsync(request.BundleId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportBundleNotFound, nameof(CatalogImportBundle));

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var searchTerm = Normalize(request.SearchTerm);

        return request.Section switch
        {
            "category-links" => Page(
                bundle.CategoryLinks
                    .Where(x => Matches(x.MappingStatus.ToString(), request.Status))
                    .Select(CatalogImportMapper.ToCategoryMappingDto)
                    .Where(x => MatchesCategoryLinkSearch(x, searchTerm))
                    .ToList(),
                pageNumber,
                pageSize),
            "sellers" => Page(
                bundle.Sellers
                    .Where(x => Matches(x.ProvisioningStatus.ToString(), request.Status))
                    .Select(CatalogImportMapper.ToSellerDto)
                    .Where(x => MatchesSellerSearch(x, searchTerm))
                    .ToList(),
                pageNumber,
                pageSize),
            "seller-ownership-candidates" => Page(
                bundle.Sellers
                    .Where(x => Matches(x.ProvisioningStatus.ToString(), request.Status))
                    .Where(x => string.IsNullOrWhiteSpace(request.SellerId) || x.Id.ToString().Equals(request.SellerId, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(CatalogImportMapper.ToExistingStoreCandidateDtos)
                    .Where(x => MatchesSearch(searchTerm, x.UserId.ToString(), x.StoreId.ToString(), x.StoreName, x.Reason))
                    .ToList(),
                pageNumber,
                pageSize),
            "products" => Page(
                bundle.Products
                    .Where(x => Matches(x.ReadinessStatus.ToString(), request.Status) || Matches(x.ImportStatus.ToString(), request.Status) || request.Status is null)
                    .Where(x => string.IsNullOrWhiteSpace(request.SellerId) || string.Equals(x.ExternalSellerId, request.SellerId, StringComparison.OrdinalIgnoreCase))
                    .Select(CatalogImportMapper.ToProductDto)
                    .Where(x => MatchesProductSearch(x, searchTerm))
                    .ToList(),
                pageNumber,
                pageSize),
            "duplicate-groups" => Page(
                bundle.DuplicateGroups
                    .Where(x => Matches(x.ResolutionStatus.ToString(), request.Status))
                    .Select(CatalogImportMapper.ToDuplicateGroupDto)
                    .Where(x => MatchesDuplicateGroupSearch(x, searchTerm))
                    .ToList(),
                pageNumber,
                pageSize),
            "validation-issues" => Page(
                bundle.ValidationIssues
                    .Where(x => Matches(x.Severity.ToString(), request.Severity))
                    .Where(x => Matches(x.EntityType, request.EntityType))
                    .Where(x => Matches(x.ReasonCode, request.ReasonCode))
                    .Select(CatalogImportMapper.ToValidationIssueDto)
                    .Where(x => MatchesValidationIssueSearch(x, searchTerm))
                    .ToList(),
                pageNumber,
                pageSize),
            _ => throw new InvalidFieldException(CatalogDomainErrorCode.InvalidCatalogImportBundle, nameof(request.Section))
        };
    }

    private static CatalogImportPagedResponseDto<T> Page<T>(IReadOnlyList<T> items, int pageNumber, int pageSize)
    {
        var data = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new CatalogImportPagedResponseDto<T>(
            data,
            CatalogImportPaginationDto.Create(pageNumber, pageSize, items.Count));
    }

    private static bool Matches(string value, string? expected)
        => string.IsNullOrWhiteSpace(expected) || string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool MatchesCategoryLinkSearch(ImportedCategoryMappingDto categoryLink, string? searchTerm)
        => MatchesSearch(
            searchTerm,
            categoryLink.CategoryLinkId.ToString(),
            categoryLink.ExternalCategoryId,
            categoryLink.CategoryId,
            categoryLink.CategoryName,
            categoryLink.ConflictReason);

    private static bool MatchesSellerSearch(ImportedSellerDto seller, string? searchTerm)
        => MatchesSearch(
            searchTerm,
            seller.ImportedSellerId.ToString(),
            seller.ExternalSellerId,
            seller.DisplayName,
            seller.UserId?.ToString(),
            seller.StoreId?.ToString());

    private static bool MatchesProductSearch(ImportedProductDto product, string? searchTerm)
        => MatchesSearch(
            searchTerm,
            product.ProductId.ToString(),
            product.ExternalProductId,
            product.ExternalSellerId,
            product.Title)
            || MatchesSearch(product.ExternalCategoryIds, searchTerm);

    private static bool MatchesDuplicateGroupSearch(ImportDuplicateGroupDto group, string? searchTerm)
        => MatchesSearch(
            searchTerm,
            group.GroupId.ToString(),
            group.CanonicalExternalProductId,
            group.ReasonCode)
            || MatchesSearch(group.ExternalProductIds, searchTerm);

    private static bool MatchesValidationIssueSearch(ImportValidationIssueDto issue, string? searchTerm)
        => MatchesSearch(
            searchTerm,
            issue.IssueId.ToString(),
            issue.EntityType,
            issue.EntitySourceId,
            issue.Field,
            issue.ReasonCode,
            issue.Message);

    private static bool MatchesSearch(string? searchTerm, params string?[] values)
    {
        if (searchTerm is null)
            return true;

        return values.Any(value => value?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static bool MatchesSearch(IEnumerable<string> values, string? searchTerm)
    {
        if (searchTerm is null)
            return true;

        return values.Any(value => value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }
}

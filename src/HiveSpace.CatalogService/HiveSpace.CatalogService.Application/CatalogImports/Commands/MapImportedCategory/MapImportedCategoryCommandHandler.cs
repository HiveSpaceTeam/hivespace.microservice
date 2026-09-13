using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.CatalogService.Domain.Repositories;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.MapImportedCategory;

public class MapImportedCategoryCommandHandler(
    ICatalogImportBundleRepository repository,
    ICategoryRepository categoryRepository)
    : ICommandHandler<MapImportedCategoryCommand, MapImportedCategoryResultDto>
{
    public async Task<MapImportedCategoryResultDto> Handle(
        MapImportedCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var bundle = await repository.GetByIdAsync(request.BundleId, cancellationToken)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CatalogImportBundleNotFound, nameof(CatalogImportBundle));

        var category = await categoryRepository.GetByIdAsync(request.HiveSpaceCategoryId)
            ?? throw new NotFoundException(CatalogDomainErrorCode.CategoryNotFound, nameof(Category));

        if (category.IsActive == false)
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(request.HiveSpaceCategoryId));

        var mapping = bundle.CategoryMappings.FirstOrDefault(x =>
                string.Equals(x.ExternalCategoryId, request.ExternalCategoryId, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException(CatalogDomainErrorCode.InvalidImportedCategory, nameof(ImportedCategoryMapping));

        mapping.MapToCategory(request.HiveSpaceCategoryId, request.MappedByUserId);
        var affectedProductCount = bundle.Products.Count(product =>
            product.ExternalCategoryIds.Contains(mapping.ExternalCategoryId, StringComparer.OrdinalIgnoreCase));

        await repository.SaveChangesAsync(cancellationToken);

        return new MapImportedCategoryResultDto(
            bundle.Id,
            mapping.ExternalCategoryId,
            request.HiveSpaceCategoryId,
            mapping.MappingStatus.ToString(),
            affectedProductCount);
    }
}

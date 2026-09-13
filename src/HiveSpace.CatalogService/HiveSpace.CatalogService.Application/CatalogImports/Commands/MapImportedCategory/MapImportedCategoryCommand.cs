using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.MapImportedCategory;

public record MapImportedCategoryCommand(
    string ExternalCategoryId,
    Guid BundleId,
    int HiveSpaceCategoryId,
    Guid MappedByUserId)
    : ICommand<MapImportedCategoryResultDto>;

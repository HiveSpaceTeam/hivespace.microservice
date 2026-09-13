using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategories;

public record ProvisionImportedCategoriesCommand(
    CategoryProvisioningRequestDto Payload,
    Guid ProvisionedByUserId,
    string? SourceFileName = null)
    : ICommand<CatalogImportJobSubmissionDto>;

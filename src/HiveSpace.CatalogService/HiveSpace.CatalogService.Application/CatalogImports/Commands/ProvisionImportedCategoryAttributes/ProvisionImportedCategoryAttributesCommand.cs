using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedCategoryAttributes;

public record ProvisionImportedCategoryAttributesCommand(
    CategoryAttributeProvisioningRequestDto Payload,
    Guid ProvisionedByUserId,
    string? SourceFileName = null)
    : ICommand<CatalogImportJobSubmissionDto>;

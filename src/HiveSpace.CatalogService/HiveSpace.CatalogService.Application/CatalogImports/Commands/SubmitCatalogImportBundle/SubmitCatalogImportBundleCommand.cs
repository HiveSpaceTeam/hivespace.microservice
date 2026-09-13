using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.SubmitCatalogImportBundle;

public record SubmitCatalogImportBundleCommand(CatalogImportBundleRequestDto Payload, string? SourceFileName = null)
    : ICommand<CatalogImportJobSubmissionDto>;

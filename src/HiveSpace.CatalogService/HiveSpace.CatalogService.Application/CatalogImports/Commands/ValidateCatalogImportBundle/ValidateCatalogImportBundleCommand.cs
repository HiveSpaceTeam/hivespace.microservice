using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ValidateCatalogImportBundle;

public record ValidateCatalogImportBundleCommand(Guid BundleId) : ICommand<CatalogImportJobSubmissionDto>;

using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedSellers;

public record ProvisionImportedSellersCommand(Guid BundleId) : ICommand<CatalogImportJobSubmissionDto>;

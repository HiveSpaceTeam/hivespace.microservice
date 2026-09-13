using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ImportReadyProducts;

public record ImportReadyProductsCommand(
    Guid BundleId,
    IReadOnlyCollection<Guid>? ProductIds,
    string PublicationState,
    Guid ImportedByUserId)
    : ICommand<CatalogImportJobSubmissionDto>;

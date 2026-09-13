namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.ImportReadyProducts;

public record ImportReadyProductsJobPayload(
    IReadOnlyCollection<Guid> ProductIds,
    string PublicationState,
    Guid ImportedByUserId,
    bool ImportAllEligibleProducts,
    IReadOnlyCollection<Guid>? RequestedProductIds);

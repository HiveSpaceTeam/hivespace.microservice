namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record ImportReadyProductsRequestDto(IReadOnlyCollection<Guid>? ProductIds, string PublicationState);

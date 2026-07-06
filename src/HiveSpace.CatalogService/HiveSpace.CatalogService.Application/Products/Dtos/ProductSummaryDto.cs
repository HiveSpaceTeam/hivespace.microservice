namespace HiveSpace.CatalogService.Application.Products.Dtos;

public record ProductSummaryDto(int Id, string Name, ProductMoneyDto Price, string? ImageURL);

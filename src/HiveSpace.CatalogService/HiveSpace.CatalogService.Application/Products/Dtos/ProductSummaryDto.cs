using HiveSpace.Application.Shared.Dtos;

namespace HiveSpace.CatalogService.Application.Products.Dtos;

public record ProductSummaryDto(int Id, string Name, MoneyResponseDto Price, string? ImageURL);

using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Application.Categories.Queries.GetAttributesByCategoryId;

public record GetAttributesByCategoryIdQuery(int CategoryId) : IQuery<List<AttributeDto>>;

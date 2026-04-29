using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Application.Categories.Queries.GetCategories;

public record GetCategoriesQuery() : IQuery<List<CategoryDto>>;

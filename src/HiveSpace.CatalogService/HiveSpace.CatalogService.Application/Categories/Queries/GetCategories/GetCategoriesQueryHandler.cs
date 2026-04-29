using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Application.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler(ICategoryDataQuery categoryDataQuery)
    : IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        => categoryDataQuery.GetCategoriesAsync(cancellationToken);
}

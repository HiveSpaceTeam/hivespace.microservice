using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Application.Categories.Queries.GetHomepageCategories;

public class GetHomepageCategoriesQueryHandler(ICategoryDataQuery categoryDataQuery)
    : IQueryHandler<GetHomepageCategoriesQuery, List<CategoryDto>>
{
    public Task<List<CategoryDto>> Handle(GetHomepageCategoriesQuery request, CancellationToken cancellationToken)
        => categoryDataQuery.GetHomepageCategoriesAsync(cancellationToken);
}

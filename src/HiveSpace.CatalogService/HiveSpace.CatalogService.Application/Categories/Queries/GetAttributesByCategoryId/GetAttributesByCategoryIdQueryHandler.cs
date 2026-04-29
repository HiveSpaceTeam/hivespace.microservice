using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Categories.Dtos;

namespace HiveSpace.CatalogService.Application.Categories.Queries.GetAttributesByCategoryId;

public class GetAttributesByCategoryIdQueryHandler(ICategoryDataQuery categoryDataQuery)
    : IQueryHandler<GetAttributesByCategoryIdQuery, List<AttributeDto>>
{
    public Task<List<AttributeDto>> Handle(GetAttributesByCategoryIdQuery request, CancellationToken cancellationToken)
        => categoryDataQuery.GetAttributesByCategoryIdAsync(request.CategoryId);
}

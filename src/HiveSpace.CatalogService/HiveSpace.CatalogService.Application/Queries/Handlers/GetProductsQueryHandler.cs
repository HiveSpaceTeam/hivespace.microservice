using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.CatalogService.Domain.Repositories.Domain;

namespace HiveSpace.CatalogService.Application.Queries.Handlers;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PagingData>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productService)
    {
        _productRepository = productService;
    }

    public async Task<PagingData> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var (items, total) = await _productRepository.GetPagedAsync(payload.Keyword ?? string.Empty, payload.PageIndex, payload.PageSize, payload.Sort, cancellationToken);
        return new PagingData(total, items);
    }
}


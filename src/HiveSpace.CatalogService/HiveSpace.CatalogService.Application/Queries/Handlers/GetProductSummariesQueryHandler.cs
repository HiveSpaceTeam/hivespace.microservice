using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Domain.Repositories;

namespace HiveSpace.CatalogService.Application.Queries.Handlers;

public class GetProductSummariesQueryHandler : IQueryHandler<GetProductSummariesQuery, PagingData>
{
    private readonly IProductRepository _productRepository;

    public GetProductSummariesQueryHandler(IProductRepository productService)
    {
        _productRepository = productService;
    }

    public async Task<PagingData> Handle(GetProductSummariesQuery request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var (items, total) = await _productRepository.GetSummariesPagedAsync(payload.Keyword ?? string.Empty, payload.PageIndex, payload.PageSize, payload.Sort, cancellationToken);

        List<ProductSummariesViewModel> viewModels = items.Select(p =>
        {
            var sku = p.Skus.FirstOrDefault();
            var image = sku?.Images.FirstOrDefault();

            var res = new ProductSummariesViewModel
            {
                Id = p.Id,
                Name = p.Name,
            };

            if (image != null)
            {
                res.ImageURL = image.FileId;
            }

            if(sku != null)
            {
                res.Price = sku.Price.Amount.ToString();
            }

            return res;

        }).ToList();

        return new PagingData(total, viewModels);
    }
}


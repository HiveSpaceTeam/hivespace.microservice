using HiveSpace.Application.Shared.Handlers;
using HiveSpace.CatalogService.Application.DataQueries;
using HiveSpace.CatalogService.Application.Models.ViewModels;
using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Application.Queries.Handlers
{
    public class GetProductDetailQueryHandler(IProductDataQuery productDataQuery) : IQueryHandler<GetProductDetailQuery, ProductDetailViewModel>
    {
        public async Task<ProductDetailViewModel> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
        {
            var productDetail = await productDataQuery.GetProductDetailViewModelAsync(request.ProductId, cancellationToken)
                ?? throw new NotFoundException(CatalogErrorCode.ProductNotFound, nameof(ProductDetailViewModel));

            return productDetail;
        }
    }
}

using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.Models.ViewModels;

namespace HiveSpace.CatalogService.Application.Queries
{
    public record GetProductDetailQuery(Guid ProductId) : IQuery<ProductDetailViewModel>;

}

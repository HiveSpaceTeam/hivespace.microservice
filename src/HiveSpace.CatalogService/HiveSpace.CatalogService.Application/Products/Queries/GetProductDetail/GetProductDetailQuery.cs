using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.Products.Dtos;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProductDetail;

public record GetProductDetailQuery(int ProductId) : IQuery<ProductDetailDto>;

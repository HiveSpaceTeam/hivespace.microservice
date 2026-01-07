using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.Models.Dtos.Crud;
using HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;

namespace HiveSpace.CatalogService.Application.Queries;

public record GetProductsQuery(ProductSearchRequestDto Payload) : IQuery<PagingData>;


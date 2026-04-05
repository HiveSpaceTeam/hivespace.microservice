using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProducts;

public record GetProductsQuery(ProductSearchRequestDto Payload) : IQuery<PagedResult<Product>>;

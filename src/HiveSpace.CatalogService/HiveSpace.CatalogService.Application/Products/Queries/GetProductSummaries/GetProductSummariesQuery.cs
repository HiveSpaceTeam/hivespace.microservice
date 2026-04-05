using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Application.Contracts;
using HiveSpace.CatalogService.Application.Products.Dtos;
using HiveSpace.Core.Models.Pagination;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProductSummaries;

public record GetProductSummariesQuery(ProductSearchRequestDto Payload) : IQuery<PagedResult<ProductSummaryDto>>;

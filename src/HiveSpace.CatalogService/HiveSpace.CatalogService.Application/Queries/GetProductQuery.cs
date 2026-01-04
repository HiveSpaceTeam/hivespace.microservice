using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Queries;

public record GetProductQuery(Guid ProductId) : IQuery<Product>;


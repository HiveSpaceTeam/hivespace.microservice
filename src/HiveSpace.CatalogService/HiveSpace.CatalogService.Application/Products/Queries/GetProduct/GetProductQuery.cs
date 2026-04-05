using HiveSpace.Application.Shared.Queries;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Products.Queries.GetProduct;

public record GetProductQuery(int ProductId) : IQuery<Product>;

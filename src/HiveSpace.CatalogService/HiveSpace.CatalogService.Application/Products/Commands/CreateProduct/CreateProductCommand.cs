using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.Contracts;

namespace HiveSpace.CatalogService.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(ProductUpsertRequestDto Payload) : ICommand<int>;

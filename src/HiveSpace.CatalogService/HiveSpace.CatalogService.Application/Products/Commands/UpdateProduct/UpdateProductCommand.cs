using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.Contracts;

namespace HiveSpace.CatalogService.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(int ProductId, ProductUpsertRequestDto Payload) : ICommand<bool>;

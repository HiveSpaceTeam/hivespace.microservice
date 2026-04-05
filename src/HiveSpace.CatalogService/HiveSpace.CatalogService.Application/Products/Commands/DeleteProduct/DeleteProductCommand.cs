using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.CatalogService.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int ProductId) : ICommand<bool>;

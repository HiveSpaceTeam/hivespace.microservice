using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.CatalogService.Application.Commands;

public record DeleteProductCommand(int ProductId) : ICommand<bool>;


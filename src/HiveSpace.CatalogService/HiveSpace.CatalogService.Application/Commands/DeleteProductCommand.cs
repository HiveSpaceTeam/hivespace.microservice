using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.CatalogService.Application.Commands;

public record DeleteProductCommand(Guid ProductId) : ICommand<bool>;


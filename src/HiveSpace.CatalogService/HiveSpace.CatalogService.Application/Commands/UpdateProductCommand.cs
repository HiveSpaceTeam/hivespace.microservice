using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.Models.Requests;

namespace HiveSpace.CatalogService.Application.Commands;

public record UpdateProductCommand(Guid ProductId, ProductUpsertRequestDto Payload) : ICommand<bool>;


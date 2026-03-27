using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.Models.Requests;

namespace HiveSpace.CatalogService.Application.Commands;

public record UpdateProductCommand(int ProductId, ProductUpsertRequestDto Payload) : ICommand<bool>;


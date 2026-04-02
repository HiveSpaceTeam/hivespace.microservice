using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;

public record AddCartItemCommand(long ProductId, long SkuId, int Quantity) : ICommand<Guid>;

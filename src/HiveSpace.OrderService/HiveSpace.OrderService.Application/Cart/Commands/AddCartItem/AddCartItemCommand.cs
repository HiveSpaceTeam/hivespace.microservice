using MediatR;

namespace HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;

public record AddCartItemCommand(long ProductId, long SkuId, int Quantity) : IRequest<Guid>;

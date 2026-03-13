using MediatR;

namespace HiveSpace.OrderService.Application.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(Guid CartItemId) : IRequest;

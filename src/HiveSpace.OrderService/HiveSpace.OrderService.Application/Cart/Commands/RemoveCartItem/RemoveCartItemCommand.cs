using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.OrderService.Application.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(Guid CartItemId) : ICommand;

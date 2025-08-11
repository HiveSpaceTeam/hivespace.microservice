using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Commands;
using HiveSpace.OrderService.Application.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using MediatR;

namespace HiveSpace.OrderService.Application.Handlers;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order is null)
            {
                throw new OrderNotFoundException(request.OrderId);
            }

            // Domain validation will be applied here
            order.UpdateStatus(request.Status);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (OrderNotFoundException)
        {
            // Re-throw specific application exceptions
            throw;
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions
            throw;
        }
        catch (Exception ex)
        {
            // Wrap unexpected exceptions
            throw new OrderApplicationException(
                [new(OrderErrorCode.InvalidOrderStatus, "Failed to update order status")],
                ex);
        }
    }
}
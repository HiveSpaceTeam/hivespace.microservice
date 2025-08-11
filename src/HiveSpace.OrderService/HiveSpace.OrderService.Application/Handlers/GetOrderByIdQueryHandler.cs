using AutoMapper;
using HiveSpace.OrderService.Application.DTOs;
using HiveSpace.OrderService.Application.Exceptions;
using HiveSpace.OrderService.Application.Queries;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using MediatR;

namespace HiveSpace.OrderService.Application.Handlers;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order is null)
            {
                throw new OrderNotFoundException(request.OrderId);
            }

            return _mapper.Map<OrderResponse>(order);
        }
        catch (OrderNotFoundException)
        {
            // Re-throw specific exceptions
            throw;
        }
        catch (Exception ex)
        {
            // Wrap unexpected exceptions
            throw new OrderApplicationException(
                [new(OrderErrorCode.OrderNotFound, "Failed to retrieve order")],
                ex);
        }
    }
}
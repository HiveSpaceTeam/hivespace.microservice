using AutoMapper;
using HiveSpace.OrderService.Application.Commands;
using HiveSpace.OrderService.Application.DTOs;
using HiveSpace.OrderService.Application.Queries;
using MediatR;

namespace HiveSpace.OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public OrderService(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Guid> CreateOrderAsync(CreateOrderRequest request)
    {
        var command = _mapper.Map<CreateOrderCommand>(request);
        return await _mediator.Send(command);
    }

    public async Task<List<OrderResponse>> GetOrdersAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters = null)
    {
        var query = new GetOrdersQuery(pageNumber, pageSize, filters);
        return await _mediator.Send(query);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid orderId)
    {
        var query = new GetOrderByIdQuery(orderId);
        return await _mediator.Send(query);
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
    {
        var command = new UpdateOrderStatusCommand(orderId, request.Status);
        return await _mediator.Send(command);
    }
}
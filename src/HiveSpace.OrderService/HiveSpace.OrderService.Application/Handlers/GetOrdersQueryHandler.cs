using AutoMapper;
using HiveSpace.OrderService.Application.DTOs;
using HiveSpace.OrderService.Application.Queries;
using HiveSpace.OrderService.Domain.Repositories;
using MediatR;

namespace HiveSpace.OrderService.Application.Handlers;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<List<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync();

        return _mapper.Map<List<OrderResponse>>(orders);
    }
}
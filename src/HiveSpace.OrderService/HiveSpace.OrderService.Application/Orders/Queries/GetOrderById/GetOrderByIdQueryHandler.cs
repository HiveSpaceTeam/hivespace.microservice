using HiveSpace.Core.Contexts;
using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Application.Orders.Mappers;
using HiveSpace.OrderService.Domain.Repositories;
using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository, IUserContext userContext)
    : IRequestHandler<GetOrderByIdQuery, OrderDetailDto?>
{
    public async Task<OrderDetailDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithPackagesAsync(request.OrderId, cancellationToken);
        if (order is null || order.UserId != userContext.UserId) return null;
        return order.ToDetailDto();
    }
}

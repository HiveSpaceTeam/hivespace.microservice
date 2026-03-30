using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Application.Orders.Mappers;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository, IUserContext userContext)
    : IRequestHandler<GetOrderByIdQuery, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithPackagesAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.OrderNotFound, nameof(Order));

        if (order.UserId != userContext.UserId)
            throw new ForbiddenException(OrderDomainErrorCode.NotOrderOwner, nameof(Order));

        return order.ToDetailDto();
    }
}

using AutoMapper;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Commands;
using HiveSpace.OrderService.Application.Exceptions;
using HiveSpace.OrderService.Domain.AggregateRoots;
using HiveSpace.OrderService.Domain.Entities;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Domain.ValueObjects;
using MediatR;

namespace HiveSpace.OrderService.Application.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate customer exists (business rule)
            if (request.CustomerId == Guid.Empty)
            {
                throw new CustomerNotFoundException();
            }

            // Map command to domain entity
            var shippingAddress = _mapper.Map<ShippingAddress>(request.ShippingAddress);
            var orderItems = _mapper.Map<List<OrderItem>>(request.Items.ToList());

            // Validate order items
            if (!orderItems.Any())
            {
                throw new DomainException(400, OrderErrorCode.InvalidOrder, nameof(Order));
            }

            // Create domain entity (domain validation will be applied here)
            var order = new Order(
                request.CustomerId,
                request.ShippingFee,
                request.Discount,
                DateTimeOffset.UtcNow,
                shippingAddress,
                request.PaymentMethod,
                orderItems
            );

            // Save to repository
            _orderRepository.Add(order);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
        catch (DomainException)
        {
            // Re-throw domain exceptions to maintain business rule violations
            throw;
        }
        catch (Exception ex)
        {
            // Log and wrap unexpected exceptions
            throw new OrderApplicationException(
                [new(OrderErrorCode.InvalidOrder, "Failed to create order")],
                ex);
        }
    }
}
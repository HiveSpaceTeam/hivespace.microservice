using HiveSpace.OrderService.Application.Orders.Dtos;
using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;

public record GetCheckoutStatusQuery(Guid CorrelationId) : IRequest<CheckoutStatusDto?>;

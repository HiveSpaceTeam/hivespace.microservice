using HiveSpace.Application.Shared.Queries;
using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;

public record GetCheckoutStatusQuery(Guid OrderId) : IQuery<CheckoutStatusDto>;

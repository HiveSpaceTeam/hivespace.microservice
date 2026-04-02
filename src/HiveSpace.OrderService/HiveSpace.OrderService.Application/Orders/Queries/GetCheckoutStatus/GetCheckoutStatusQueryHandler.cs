using HiveSpace.Application.Shared.Handlers;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;

public class GetCheckoutStatusQueryHandler(ICheckoutQuery checkoutQuery)
    : IQueryHandler<GetCheckoutStatusQuery, CheckoutStatusDto>
{
    public Task<CheckoutStatusDto> Handle(GetCheckoutStatusQuery request, CancellationToken cancellationToken)
        => checkoutQuery.GetCheckoutStatusAsync(request.OrderId, cancellationToken);
}

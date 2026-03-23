using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;
using HiveSpace.OrderService.Infrastructure.Data;
using HiveSpace.OrderService.Infrastructure.Sagas;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.QueryHandlers;

public class GetCheckoutStatusQueryHandler(OrderDbContext db)
    : IRequestHandler<GetCheckoutStatusQuery, CheckoutStatusDto?>
{
    public async Task<CheckoutStatusDto?> Handle(GetCheckoutStatusQuery request, CancellationToken cancellationToken)
    {
        var saga = await db.Set<CheckoutSagaState>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CorrelationId == request.CorrelationId, cancellationToken);

        if (saga is null) return null;

        return new CheckoutStatusDto
        {
            CorrelationId = saga.CorrelationId,
            CurrentState  = saga.CurrentState,
            OrderId       = saga.OrderId == default ? null : saga.OrderId,
            FailureReason = saga.FailureReason,
            IsCompleted   = saga.CurrentState == "Completed",
            IsFailed      = saga.CurrentState == "Failed"
        };
    }
}

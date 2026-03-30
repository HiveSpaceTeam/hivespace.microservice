using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Orders.Dtos;
using HiveSpace.OrderService.Application.Orders.Queries.GetCheckoutStatus;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Infrastructure.Data;
using HiveSpace.OrderService.Infrastructure.Sagas;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.OrderService.Infrastructure.QueryHandlers;

public class GetCheckoutStatusQueryHandler(OrderDbContext db)
    : IRequestHandler<GetCheckoutStatusQuery, CheckoutStatusDto>
{
    public async Task<CheckoutStatusDto> Handle(GetCheckoutStatusQuery request, CancellationToken cancellationToken)
    {
        var saga = await db.Set<CheckoutSagaState>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CorrelationId == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(OrderDomainErrorCode.OrderNotFound, nameof(CheckoutSagaState));

        return new CheckoutStatusDto
        {
            CorrelationId = saga.CorrelationId,
            CurrentState  = saga.CurrentState,
            OrderId       = saga.CorrelationId,
            FailureReason = saga.FailureReason,
            IsCompleted   = saga.CurrentState == "Completed",
            IsFailed      = saga.CurrentState == "Failed"
        };
    }
}

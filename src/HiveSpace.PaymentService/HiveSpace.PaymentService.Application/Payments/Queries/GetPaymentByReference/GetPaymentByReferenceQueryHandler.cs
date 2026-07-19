using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Payments.Dtos;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByReference;

public class GetPaymentByReferenceQueryHandler(IPaymentRepository paymentRepository, IUserContext userContext)
    : IQueryHandler<GetPaymentByReferenceQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentByReferenceQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByReferenceNoAsync(request.ReferenceNo, cancellationToken)
            ?? throw new NotFoundException(PaymentDomainErrorCode.PaymentNotFound, nameof(Payment));

        if (!GetPaymentQueryHandler.CanRead(payment, userContext))
            throw new ForbiddenException(PaymentDomainErrorCode.PaymentAccessForbidden, nameof(Payment));

        return GetPaymentQueryHandler.ToDto(payment, GetPaymentQueryHandler.IncludeAttemptHistory(userContext));
    }
}

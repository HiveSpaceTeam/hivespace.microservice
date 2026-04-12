using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Payments.Dtos;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler(IPaymentRepository paymentRepository)
    : IQueryHandler<GetPaymentByOrderIdQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(PaymentDomainErrorCode.PaymentNotFound, nameof(Payment));

        return GetPaymentQueryHandler.ToDto(payment);
    }
}

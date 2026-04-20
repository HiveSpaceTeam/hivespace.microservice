using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Application.Payments.Dtos;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Repositories;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;

public class GetPaymentQueryHandler(IPaymentRepository paymentRepository, IUserContext userContext)
    : IQueryHandler<GetPaymentQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(PaymentDomainErrorCode.PaymentNotFound, nameof(Payment));

        //if (payment.BuyerId != userContext.UserId)
        //    throw new ForbiddenException(PaymentDomainErrorCode.PaymentAccessForbidden, nameof(Payment));

        return ToDto(payment);
    }

    internal static PaymentDto ToDto(Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.BuyerId,
        payment.Amount.Amount,
        payment.Amount.Currency.ToString(),
        payment.Status.ToString(),
        payment.Gateway.ToString(),
        payment.GatewayTransactionId,
        payment.GatewayPaymentUrl,
        payment.PaidAt,
        payment.ExpiresAt,
        payment.CreatedAt);
}

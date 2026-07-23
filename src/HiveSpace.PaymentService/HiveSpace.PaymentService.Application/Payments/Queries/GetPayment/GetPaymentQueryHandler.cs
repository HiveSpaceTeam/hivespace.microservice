using HiveSpace.Application.Shared.Dtos;
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

        if (!CanRead(payment, userContext))
            throw new ForbiddenException(PaymentDomainErrorCode.PaymentAccessForbidden, nameof(Payment));

        return ToDto(payment, IncludeAttemptHistory(userContext));
    }

    internal static bool CanRead(Payment payment, IUserContext userContext) =>
        userContext.IsAdmin || userContext.IsSystemAdmin || payment.BuyerId == userContext.UserId;

    internal static bool IncludeAttemptHistory(IUserContext userContext) =>
        userContext.IsAdmin || userContext.IsSystemAdmin;

    internal static PaymentDto ToDto(Payment payment, bool includeAttemptHistory = false) => new(
        payment.Id,
        payment.OrderId,
        payment.BuyerId,
        MoneyResponseDto.Valid(payment.Amount.Amount, payment.Amount.Currency.ToString()),
        payment.Status.ToString(),
        payment.Gateway.ToString(),
        payment.GatewayTransactionId,
        payment.GatewayPaymentUrl,
        payment.PaidAt,
        payment.ExpiresAt,
        payment.CreatedAt,
        payment.ReferenceNo,
        payment.MethodCode,
        payment.LinkedOrders
            .Select(order => new PaymentLinkedOrderDto(
                order.OrderId,
                order.OrderCode,
                order.StoreId,
                MoneyResponseDto.Valid(order.Amount, order.CurrencyCode),
                order.StatusSnapshot))
            .ToList(),
        payment.CurrentAttempt is null ? null : ToAttemptDto(payment.CurrentAttempt),
        includeAttemptHistory
            ? payment.Attempts
                .OrderBy(attempt => attempt.AttemptNo)
                .Select(ToAttemptDto)
                .ToList()
            : null);

    private static PaymentAttemptDto ToAttemptDto(PaymentAttempt attempt) => new(
        attempt.Id,
        attempt.AttemptNo,
        attempt.MethodCode,
        attempt.GatewayCode,
        attempt.Status.ToString(),
        attempt.RedirectUrl,
        attempt.GatewayTransactionId,
        attempt.FailureReasonCode,
        attempt.CreatedAt,
        attempt.ExpiresAt,
        attempt.CompletedAt);
}

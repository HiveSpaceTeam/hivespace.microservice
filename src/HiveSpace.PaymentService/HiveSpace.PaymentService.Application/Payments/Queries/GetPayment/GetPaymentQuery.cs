using HiveSpace.Application.Shared.Queries;
using HiveSpace.PaymentService.Application.Payments.Dtos;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPayment;

public record GetPaymentQuery(Guid PaymentId) : IQuery<PaymentDto>;

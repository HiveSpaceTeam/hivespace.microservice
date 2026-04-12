using HiveSpace.Application.Shared.Queries;
using HiveSpace.PaymentService.Application.Payments.Dtos;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IQuery<PaymentDto>;

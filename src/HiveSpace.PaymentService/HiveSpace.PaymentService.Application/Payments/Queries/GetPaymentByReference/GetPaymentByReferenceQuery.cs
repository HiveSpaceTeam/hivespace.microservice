using HiveSpace.Application.Shared.Queries;
using HiveSpace.PaymentService.Application.Payments.Dtos;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentByReference;

public record GetPaymentByReferenceQuery(string ReferenceNo) : IQuery<PaymentDto>;

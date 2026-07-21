using HiveSpace.Application.Shared.Queries;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentMethods.Dtos;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentMethods;

public record GetPaymentMethodsQuery : IQuery<GetPaymentMethodsResponse>;

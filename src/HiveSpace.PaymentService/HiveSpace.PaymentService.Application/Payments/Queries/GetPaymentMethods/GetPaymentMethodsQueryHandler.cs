using HiveSpace.Application.Shared.Handlers;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentMethods.Dtos;
using PaymentMethodCodes = HiveSpace.Domain.Shared.Enumerations.PaymentMethodCodes;

namespace HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentMethods;

public class GetPaymentMethodsQueryHandler : IQueryHandler<GetPaymentMethodsQuery, GetPaymentMethodsResponse>
{
    public Task<GetPaymentMethodsResponse> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<PaymentMethodMetadataDto> methods =
        [
            new(PaymentMethodCodes.COD, "Cash on delivery", "Offline", null, true, true, "Available", 10),
            new(PaymentMethodCodes.VNPAY, "VNPay", "Online", PaymentMethodCodes.VNPAY, true, true, "Available", 20),
            new(PaymentMethodCodes.Stripe, "Stripe", "Online", PaymentMethodCodes.Stripe, false, false, "Future", 30)
        ];

        return Task.FromResult(new GetPaymentMethodsResponse(methods));
    }
}

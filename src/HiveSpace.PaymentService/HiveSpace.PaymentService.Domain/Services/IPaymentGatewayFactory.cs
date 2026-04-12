using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;

namespace HiveSpace.PaymentService.Domain.Services;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(PaymentGateway gateway);
}

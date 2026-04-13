using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Services;

namespace HiveSpace.PaymentService.Infrastructure.Gateways;

public class PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways) : IPaymentGatewayFactory
{
    private readonly IReadOnlyDictionary<PaymentGateway, IPaymentGateway> _gateways =
        gateways.ToDictionary(g => g.GatewayType);

    public IPaymentGateway GetGateway(PaymentGateway gateway)
        => _gateways.TryGetValue(gateway, out var g)
            ? g
            : throw new InvalidFieldException(PaymentDomainErrorCode.GatewayNotSupported, nameof(gateway));
}

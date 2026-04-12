using MassTransit;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class ClearCartConsumerDefinition : ConsumerDefinition<ClearCartConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ClearCartConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.None());
    }
}

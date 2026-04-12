using MassTransit;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class MarkOrderAsCODConsumerDefinition : ConsumerDefinition<MarkOrderAsCODConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<MarkOrderAsCODConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.None());
    }
}

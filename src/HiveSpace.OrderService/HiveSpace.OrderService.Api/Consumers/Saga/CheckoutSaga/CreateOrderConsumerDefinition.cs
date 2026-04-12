using MassTransit;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class CreateOrderConsumerDefinition : ConsumerDefinition<CreateOrderConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<CreateOrderConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // This is a saga request/response step — the saga handles OrderCreation.Faulted
        // for compensation, so retrying here only delays fault propagation.
        endpointConfigurator.UseMessageRetry(r => r.None());
    }
}

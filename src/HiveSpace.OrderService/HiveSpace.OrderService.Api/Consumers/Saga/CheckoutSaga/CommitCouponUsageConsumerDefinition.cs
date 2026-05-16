using MassTransit;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class CommitCouponUsageConsumerDefinition : ConsumerDefinition<CommitCouponUsageConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<CommitCouponUsageConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.None());
    }
}

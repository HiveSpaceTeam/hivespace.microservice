using MassTransit;

namespace HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;

public class CancelOrderConsumerDefinition : ConsumerDefinition<CancelOrderConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<CancelOrderConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        // Compensation step — safe to retry. The CancelOrderCommandHandler is idempotent
        // (already-cancelled orders are skipped). Retries here handle SQL Server deadlocks
        // that can occur when the saga's pessimistic-lock transaction and this consumer's
        // transaction contend for the same outbox/order rows.
        //
        // Deadlock exception chain: InvalidOperationException → DbUpdateException → SqlException(1205).
        // Handling InvalidOperationException catches both EF Core transient-error wrappers:
        //   "Consider enabling retry on failure" and "does not support user-initiated transactions".
        endpointConfigurator.UseMessageRetry(r =>
            r.Intervals(500, 1000, 2000, 5000)
             .Handle<InvalidOperationException>());
    }
}

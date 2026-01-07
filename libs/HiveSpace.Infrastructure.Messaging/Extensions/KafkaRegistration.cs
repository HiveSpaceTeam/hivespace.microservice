using System;
using MassTransit;
using MassTransit.KafkaIntegration;

namespace HiveSpace.Infrastructure.Messaging.Extensions;

internal sealed record KafkaRegistration(
    Action<IRiderRegistrationConfigurator>? ConfigureRider,
    Action<IKafkaFactoryConfigurator, IRiderRegistrationContext>? ConfigureEndpoints);



using FluentAssertions;
using HiveSpace.IdentityService.Api.Grpc;
using Xunit;

namespace HiveSpace.IdentityService.Tests.Api.Consumers;

public class StoreCreatedConsumerRemovalTests
{
    [Fact]
    public void StoreCreatedIntegrationEvent_Replayed_DoesNotAssignSellerRole()
    {
        var apiAssembly = typeof(IdentitySellerProvisioningGrpcService).Assembly;

        apiAssembly.GetTypes()
            .Should()
            .NotContain(type => type.Name == "StoreCreatedConsumer");
    }
}

using FluentAssertions;
using Grpc.Core;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc.SellerProvisioning;
using HiveSpace.UserService.Application.Stores.IdentityProvisioning;
using HiveSpace.UserService.Infrastructure.IdentityProvisioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace HiveSpace.UserService.Tests.Infrastructure.IdentityProvisioning;

public class IdentitySellerProvisioningGrpcClientTests
{
    [Fact]
    public async Task ProvisionAsync_WithConflictOutcome_ReturnsFailedRegistrationDetails()
    {
        await using var server = await StubServer.StartAsync(_ => Task.FromResult(new ProvisionSellerIdentityResponse
        {
            Outcome = ProvisioningOutcome.Conflict,
            FailureCode = "StoreConflict",
            FailureMessage = "Different store already linked."
        }));
        var sut = CreateClient(server.Address);

        var result = await sut.ProvisionSellerIdentityAsync(Guid.NewGuid(), Guid.NewGuid(), "Store", "corr");

        result.Succeeded.Should().BeFalse();
        result.Outcome.Should().Be(IdentitySellerProvisioningOutcome.Conflict);
        result.FailureCode.Should().Be("StoreConflict");
    }

    [Fact]
    public async Task ProvisionAsync_WithDeadlineExceeded_ReturnsTimeoutFailure()
    {
        await using var server = await StubServer.StartAsync(async _ =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return new ProvisionSellerIdentityResponse { Outcome = ProvisioningOutcome.Created };
        });
        var sut = CreateClient(server.Address, deadlineSeconds: 1);

        var result = await sut.ProvisionSellerIdentityAsync(Guid.NewGuid(), Guid.NewGuid(), "Store", "corr");

        result.Succeeded.Should().BeFalse();
        result.Outcome.Should().Be(IdentitySellerProvisioningOutcome.Timeout);
        result.FailureCode.Should().Be("IdentityProvisioningTimeout");
    }

    [Fact]
    public async Task ProvisionAsync_WithCreatedOutcome_SendsCorrelationIdAndBearerToken()
    {
        ProvisionSellerIdentityRequest? received = null;
        await using var server = await StubServer.StartAsync(request =>
        {
            received = request;
            return Task.FromResult(new ProvisionSellerIdentityResponse { Outcome = ProvisioningOutcome.Created });
        });
        var sut = CreateClient(server.Address);

        var result = await sut.ProvisionSellerIdentityAsync(Guid.NewGuid(), Guid.NewGuid(), "Store", "corr-9");

        result.Succeeded.Should().BeTrue();
        received!.CorrelationId.Should().Be("corr-9");
    }

    private static IdentitySellerProvisioningGrpcClient CreateClient(string address, int? deadlineSeconds = null)
    {
        var settings = new Dictionary<string, string?> { ["InternalGrpc:IdentityService:Address"] = address };
        if (deadlineSeconds is not null)
        {
            settings["InternalGrpc:IdentityService:DeadlineSeconds"] = deadlineSeconds.Value.ToString();
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var tokenProvider = Substitute.For<IIdentityServiceClientTokenProvider>();
        tokenProvider.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("token");
        return new IdentitySellerProvisioningGrpcClient(configuration, tokenProvider);
    }

    private sealed class StubServer : IAsyncDisposable
    {
        private readonly WebApplication _app;
        public string Address { get; }

        private StubServer(WebApplication app, string address)
        {
            _app = app;
            Address = address;
        }

        public static async Task<StubServer> StartAsync(
            Func<ProvisionSellerIdentityRequest, Task<ProvisionSellerIdentityResponse>> handler)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(o =>
                o.Listen(System.Net.IPAddress.Loopback, 0, l => l.Protocols = HttpProtocols.Http2));
            builder.Services.AddGrpc();
            builder.Services.AddSingleton(new Handler(handler));
            var app = builder.Build();
            app.MapGrpcService<StubService>();
            await app.StartAsync();

            var address = app.Urls.First();
            return new StubServer(app, address);
        }

        public async ValueTask DisposeAsync()
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    private sealed record Handler(Func<ProvisionSellerIdentityRequest, Task<ProvisionSellerIdentityResponse>> Invoke);

    private sealed class StubService(Handler handler) : IdentitySellerProvisioning.IdentitySellerProvisioningBase
    {
        public override Task<ProvisionSellerIdentityResponse> ProvisionSellerIdentity(
            ProvisionSellerIdentityRequest request,
            ServerCallContext context)
            => handler.Invoke(request);
    }
}

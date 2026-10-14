using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HiveSpace.UserService.Infrastructure.IdentityProvisioning;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HiveSpace.UserService.Tests.Infrastructure.IdentityProvisioning;

public class IdentityServiceClientTokenProviderTests
{
    [Fact]
    public async Task GetAccessTokenAsync_RequestsClientCredentialsTokenAndCachesIt()
    {
        var handler = new RecordingHandler();
        handler.EnqueueJson(new
        {
            access_token = "token-1",
            expires_in = 1800,
            token_type = "Bearer"
        });
        var provider = new IdentityServiceClientTokenProvider(
            new HttpClient(handler),
            NewConfiguration());

        var first = await provider.GetAccessTokenAsync();
        var second = await provider.GetAccessTokenAsync();

        first.Should().Be("token-1");
        second.Should().Be("token-1");
        handler.Requests.Should().HaveCount(1);
        handler.Requests[0].RequestUri!.ToString().Should().Be("http://localhost:5001/connect/token");
        var body = handler.RequestBodies.Single();
        body.Should().Contain("grant_type=client_credentials");
        body.Should().Contain("client_id=user-service");
        body.Should().Contain("client_secret=secret");
        body.Should().Contain("scope=identity.fullaccess");
    }

    private static IConfiguration NewConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["InternalGrpc:IdentityService:TokenEndpoint"] = "http://localhost:5001/connect/token",
                ["InternalGrpc:IdentityService:ClientId"] = "user-service",
                ["InternalGrpc:IdentityService:ClientSecret"] = "secret",
                ["InternalGrpc:IdentityService:Scope"] = "identity.fullaccess"
            })
            .Build();

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> RequestBodies { get; } = [];

        public void EnqueueJson<T>(T value)
        {
            _responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(value)
            });
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            RequestBodies.Add(request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken));

            return _responses.Dequeue();
        }
    }
}

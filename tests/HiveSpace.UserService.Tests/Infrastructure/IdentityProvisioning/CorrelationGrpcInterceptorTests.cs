using FluentAssertions;
using Grpc.Core;
using HiveSpace.Infrastructure.Messaging.Shared.Grpc;
using Xunit;

namespace HiveSpace.UserService.Tests.Infrastructure.IdentityProvisioning;

public class CorrelationGrpcInterceptorTests
{
    private static readonly Method<string, string> TestMethod = new(
        MethodType.Unary,
        "svc",
        "method",
        Marshallers.Create(s => System.Text.Encoding.UTF8.GetBytes(s), b => System.Text.Encoding.UTF8.GetString(b)),
        Marshallers.Create(s => System.Text.Encoding.UTF8.GetBytes(s), b => System.Text.Encoding.UTF8.GetString(b)));

    [Fact]
    public void AsyncUnaryCall_WithExplicitCorrelationId_AddsHeader()
    {
        var sut = new CorrelationGrpcInterceptor("corr-1");

        var headers = Invoke(sut, new CallOptions());

        headers.GetValue(CorrelationGrpcInterceptor.HeaderName).Should().Be("corr-1");
    }

    [Fact]
    public void AsyncUnaryCall_WithExistingHeader_DoesNotOverwrite()
    {
        var sut = new CorrelationGrpcInterceptor("corr-1");
        var existing = new Metadata { { CorrelationGrpcInterceptor.HeaderName, "already-set" } };

        var headers = Invoke(sut, new CallOptions(headers: existing));

        headers.GetAll(CorrelationGrpcInterceptor.HeaderName).Should().ContainSingle()
            .Which.Value.Should().Be("already-set");
    }

    [Fact]
    public void AsyncUnaryCall_WithoutCorrelationId_GeneratesOne()
    {
        var sut = new CorrelationGrpcInterceptor();

        var headers = Invoke(sut, new CallOptions());

        headers.GetValue(CorrelationGrpcInterceptor.HeaderName).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UnaryServerHandler_EchoesIncomingCorrelationIdInResponseHeaders()
    {
        var sut = new CorrelationGrpcInterceptor();
        var context = new FakeServerCallContext(
            new Metadata { { CorrelationGrpcInterceptor.HeaderName, "corr-2" } });

        var response = await sut.UnaryServerHandler<string, string>(
            "req", context, (r, _) => Task.FromResult(r + "-ok"));

        response.Should().Be("req-ok");
        context.WrittenHeaders!.GetValue(CorrelationGrpcInterceptor.HeaderName).Should().Be("corr-2");
    }

    private static Metadata Invoke(CorrelationGrpcInterceptor sut, CallOptions options)
    {
        Metadata? captured = null;
        var context = new Grpc.Core.Interceptors.ClientInterceptorContext<string, string>(TestMethod, null, options);

        sut.AsyncUnaryCall<string, string>("req", context, (_, ctx) =>
        {
            captured = ctx.Options.Headers;
            return new AsyncUnaryCall<string>(
                Task.FromResult("ok"),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        });

        return captured!;
    }

    private sealed class FakeServerCallContext(Metadata requestHeaders) : ServerCallContext
    {
        public Metadata? WrittenHeaders { get; private set; }

        protected override string MethodCore => "method";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "peer";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => requestHeaders;
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore { get; } = new();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => new(null, new Dictionary<string, List<AuthProperty>>());

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
            => throw new NotSupportedException();

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            WrittenHeaders = responseHeaders;
            return Task.CompletedTask;
        }
    }
}

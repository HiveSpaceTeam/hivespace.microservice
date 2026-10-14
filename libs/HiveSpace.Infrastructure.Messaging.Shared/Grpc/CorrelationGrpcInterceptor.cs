using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace HiveSpace.Infrastructure.Messaging.Shared.Grpc;

public class CorrelationGrpcInterceptor(string? correlationId = null) : Interceptor
{
    public const string HeaderName = "x-correlation-id";

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = context.Options.Headers ?? new Metadata();
        if (headers.Get(HeaderName) is null)
        {
            headers.Add(HeaderName, Resolve(correlationId));
        }

        var options = context.Options.WithHeaders(headers);
        return continuation(request, new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options));
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var id = Resolve(context.RequestHeaders.GetValue(HeaderName));

        Activity.Current?.SetTag("correlation_id", id);
        await context.WriteResponseHeadersAsync(new Metadata { { HeaderName, id } });

        return await continuation(request, context);
    }

    private static string Resolve(string? explicitId)
        => !string.IsNullOrWhiteSpace(explicitId)
            ? explicitId
            : Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
}

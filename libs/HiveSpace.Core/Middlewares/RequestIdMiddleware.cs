using HiveSpace.Core.Contexts;
using Microsoft.AspNetCore.Http;

namespace HiveSpace.Core.Middlewares;

public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRequestContext requestContext)
    {
        // Try to get from header, otherwise generate a new one
        var requestId = context.Request.Headers["Idempotency-Key"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();

        requestContext.RequestId = requestId;

        await _next(context);
    }
}
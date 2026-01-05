using System.Net;
using HiveSpace.Core.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace HiveSpace.Core.Filters;

public class GlobalFunctionExceptionMiddleware(ILogger<GlobalFunctionExceptionMiddleware> logger) : IFunctionsWorkerMiddleware
{
    private readonly ILogger<GlobalFunctionExceptionMiddleware> _logger = logger;

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invocation");

            var request = await context.GetHttpRequestDataAsync();
            if (request != null)
            {
                var errorResponse = ExceptionResponseFactory.CreateResponse(ex);
                
                var response = request.CreateResponse();
                response.StatusCode = (HttpStatusCode)int.Parse(errorResponse.Status);
                await response.WriteAsJsonAsync(errorResponse);
                
                context.GetInvocationResult().Value = response;
            }
            else
            {
                // Re-throw for non-HTTP triggers to preserve retry/dead-letter behavior
                throw;
            }
        }
    }
}

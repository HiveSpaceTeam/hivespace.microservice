using HiveSpace.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace HiveSpace.Core.Extensions;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseHiveSpaceExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var feature   = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature?.Error;
                if (exception != null)
                {
                    var errorResponse = ExceptionResponseFactory.CreateResponse(exception);
                    context.Response.StatusCode  = int.TryParse(errorResponse.Status, out var code) ? code : 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(errorResponse);
                }
            });
        });
        return app;
    }
}

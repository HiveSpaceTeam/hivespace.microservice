using HiveSpace.Core;
using HiveSpace.Core.Middlewares;
using HiveSpace.OrderService.Api.Endpoints;
using HiveSpace.OrderService.Infrastructure;
using HiveSpace.OrderService.Infrastructure.Data;

namespace HiveSpace.OrderService.Api.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAppApiControllers();
        builder.Services.AddAppSwagger();
        builder.Services.AddOrderDbContext(builder.Configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddIdGenerators(builder.Configuration);
        builder.Services.AddAppMessaging(builder.Configuration);
        builder.Services.AddAppAuthentication(builder.Configuration);
        builder.Services.AddAppMediatR();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HiveSpace.OrderService API v1 - Org"));
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<RequestIdMiddleware>();
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var feature   = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                var exception = feature?.Error;
                if (exception != null)
                {
                    var errorResponse = HiveSpace.Core.Helpers.ExceptionResponseFactory.CreateResponse(exception);
                    context.Response.StatusCode  = int.Parse(errorResponse.Status);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(errorResponse);
                }
            });
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapCouponEndpoints();
        app.MapCartEndpoints();
        app.MapOrderEndpoints();

        return app;
    }
}

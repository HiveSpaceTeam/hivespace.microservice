using HiveSpace.Core;
using HiveSpace.Core.Extensions;
using HiveSpace.Core.Middlewares;
using HiveSpace.PaymentService.Api.Endpoints;
using HiveSpace.PaymentService.Application;
using HiveSpace.PaymentService.Infrastructure;
using Scalar.AspNetCore;

namespace HiveSpace.PaymentService.Api.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAppApiControllers();
        builder.Services.AddAppOpenApi();
        builder.Services.AddPaymentDbContext(builder.Configuration);
        builder.Services.AddCoreServices();
        builder.Services.AddAppMessaging(builder.Configuration);
        builder.Services.AddAppAuthentication(builder.Configuration);
        builder.Services.AddApplication();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference(options => options
                .WithTitle("HiveSpace PaymentService API")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<RequestIdMiddleware>();
        app.UseHiveSpaceExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapPaymentEndpoints();
        app.MapWalletEndpoints();

        return app;
    }
}

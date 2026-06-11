using HiveSpace.MediaService.Api.Extensions;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Infrastructure;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Applying MediaService migrations");
    await DataSeeder.EnsureSeedDataAsync(app);
    var allowedOrigins = GetAllowedOrigins(app.Configuration);
    logger.LogInformation("Configuring blob storage CORS for origins: {AllowedOrigins}", allowedOrigins);
    await services.GetRequiredService<IStorageService>()
        .ConfigureCorsAsync(allowedOrigins, app.Lifetime.ApplicationStopping);
    logger.LogInformation("MediaService migrations are ready");
}

app.Run();

static string[] GetAllowedOrigins(IConfiguration configuration)
{
    return configuration.GetSection("AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:5173", "http://localhost:5174", "http://localhost:5175"];
}

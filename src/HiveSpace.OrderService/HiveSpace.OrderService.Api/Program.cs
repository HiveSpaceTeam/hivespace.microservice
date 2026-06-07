using HiveSpace.OrderService.Api.Extensions;
using HiveSpace.OrderService.Infrastructure;
using HiveSpace.OrderService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Applying OrderService migrations and seed data");
    await DataSeeder.EnsureSeedDataAsync(app);
    logger.LogInformation("OrderService migrations and seed data are ready");
}

await app.RunAsync();

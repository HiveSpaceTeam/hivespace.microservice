using HiveSpace.PaymentService.Api.Extensions;
using HiveSpace.PaymentService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Applying PaymentService migrations and seed data");
    await DataSeeder.EnsureSeedDataAsync(app);
    logger.LogInformation("PaymentService migrations and seed data are ready");
}

await app.RunAsync();

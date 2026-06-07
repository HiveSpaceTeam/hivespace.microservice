using HiveSpace.NotificationService.Api.Extensions;
using HiveSpace.NotificationService.Core;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Applying NotificationService migrations and seed data");
    await DataSeeder.EnsureSeedDataAsync(app);
    logger.LogInformation("NotificationService migrations and seed data are ready");
}

app.Run();

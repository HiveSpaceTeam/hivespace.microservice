using HiveSpace.IdentityService.Api.Extensions;
using HiveSpace.IdentityService.Core;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Applying IdentityService migrations and seed data");
    await DataSeeder.EnsureSeedDataAsync(app);
    logger.LogInformation("IdentityService migrations and seed data are ready");
}

app.Run();

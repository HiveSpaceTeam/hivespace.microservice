using HiveSpace.MediaService.Api.Extensions;
using HiveSpace.MediaService.Core.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Applying MediaService migrations");
    await DataSeeder.EnsureSeedDataAsync(app);
    logger.LogInformation("MediaService migrations are ready");
}

app.Run();

using HiveSpace.CatalogService.Api.Extensions;
using HiveSpace.CatalogService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Applying CatalogService migrations and seed data");
    await DataSeeder.EnsureSeedDataAsync(app);
    logger.LogInformation("CatalogService migrations and seed data are ready");
}

await app.RunAsync();

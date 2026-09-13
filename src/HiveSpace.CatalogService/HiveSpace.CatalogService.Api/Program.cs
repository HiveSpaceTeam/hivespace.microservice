using HiveSpace.CatalogService.Api.Extensions;
using HiveSpace.CatalogService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (!app.Environment.IsProduction())
{
    var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", true);
    var seedSampleData = app.Configuration.GetValue("Seeding:SampleDataEnabled", false);
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Initializing CatalogService database. AutoMigrate: {AutoMigrate}, SampleDataEnabled: {SampleDataEnabled}",
        autoMigrate, seedSampleData);
    await DataSeeder.InitializeAsync(app, autoMigrate, seedSampleData);
    logger.LogInformation("CatalogService database initialization is ready");
}

await app.RunAsync();

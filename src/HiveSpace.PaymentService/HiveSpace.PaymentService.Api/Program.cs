using HiveSpace.PaymentService.Api.Extensions;
using HiveSpace.PaymentService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (!app.Environment.IsProduction())
{
    var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", true);
    var seedSampleData = app.Configuration.GetValue("Seeding:SampleDataEnabled", false);
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Initializing PaymentService database. AutoMigrate: {AutoMigrate}, SampleDataEnabled: {SampleDataEnabled}",
        autoMigrate, seedSampleData);
    await DataSeeder.InitializeAsync(app, autoMigrate, seedSampleData);
    logger.LogInformation("PaymentService database initialization is ready");
}

await app.RunAsync();

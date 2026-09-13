using HiveSpace.UserService.Api.Extensions;
using HiveSpace.UserService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder
    .ConfigureServices(builder.Configuration)
    .ConfigurePipeline();

if (!app.Environment.IsProduction())
{
    var autoMigrate = app.Configuration.GetValue("Database:AutoMigrate", true);
    var seedSampleData = app.Configuration.GetValue("Seeding:SampleDataEnabled", false);
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Initializing UserService database. AutoMigrate: {AutoMigrate}, SampleDataEnabled: {SampleDataEnabled}",
        autoMigrate, seedSampleData);
    await DataSeeder.InitializeAsync(app, autoMigrate, seedSampleData);
    logger.LogInformation("UserService database initialization is ready");
}

app.Run();

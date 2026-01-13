using HiveSpace.CatalogService.API.Extentions;
using HiveSpace.CatalogService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var app = builder
    .ConfigureServices(configuration)
    .ConfigurePipeline();

try 
{
    Console.WriteLine("Starting Catalog Service...");
    
    // this seeding is only for the template to bootstrap the DB and users.
    // in production you will likely want a different approach.
    if (app.Environment.IsDevelopment())
    {
         Console.WriteLine("Attempting to seed data...");
         await SeedData.EnsureSeedDataAsync(app);
         Console.WriteLine("Seed data completed.");
    }

    Console.WriteLine("Running application...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Unhandled exception: {ex}");
    throw;
}
finally
{
    Console.WriteLine("Application shutdown complete.");
}

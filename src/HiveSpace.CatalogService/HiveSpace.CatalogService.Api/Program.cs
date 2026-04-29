using HiveSpace.CatalogService.Api.Extensions;
using HiveSpace.CatalogService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Attempting to seed data...");
    await DataSeeder.EnsureSeedDataAsync(app);
}

await app.ConfigurePipeline().RunAsync();

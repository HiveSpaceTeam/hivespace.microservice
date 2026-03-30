using HiveSpace.OrderService.Api.Extensions;
using HiveSpace.OrderService.Infrastructure;
using HiveSpace.OrderService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Attempting to run database migrations...");
    await SeedData.EnsureSeedDataAsync(app);
    Console.WriteLine("Database migrations completed.");
}

await app.ConfigurePipeline().RunAsync();

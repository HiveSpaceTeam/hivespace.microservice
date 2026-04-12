using HiveSpace.PaymentService.Api.Extensions;
using HiveSpace.PaymentService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Attempting to run database migrations...");
    await DataSeeder.EnsureSeedDataAsync(app);
    Console.WriteLine("Database migrations completed.");
}

await app.ConfigurePipeline().RunAsync();

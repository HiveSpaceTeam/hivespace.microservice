using HiveSpace.UserService.Api.Extensions;
using HiveSpace.UserService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder
    .ConfigureServices(builder.Configuration)
    .ConfigurePipeline();

if (app.Environment.IsDevelopment())
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Ensuring UserService seed data");
    await DataSeeder.EnsureSeedDataAsync(app);
    logger.LogInformation("UserService seed data is ready");
}

app.Run();

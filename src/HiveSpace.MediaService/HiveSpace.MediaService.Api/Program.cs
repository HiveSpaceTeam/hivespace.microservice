using HiveSpace.MediaService.Api.Extensions;
using HiveSpace.MediaService.Core.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.ConfigureServices();
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
    await DataSeeder.EnsureSeedDataAsync(app);

app.Run();

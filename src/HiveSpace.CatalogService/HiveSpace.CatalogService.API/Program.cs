using HiveSpace.CatalogService.API.Extentions;
using HiveSpace.CatalogService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var app = builder
    .ConfigureServices(configuration)
    .ConfigurePipeline();

// this seeding is only for the template to bootstrap the DB and users.
// in production you will likely want a different approach.
if (app.Environment.IsDevelopment())
{
    SeedData.EnsureSeedData(app);
}

app.Run();

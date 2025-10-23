using HiveSpace.CatalogService.API.Extentions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var app = builder
    .ConfigureServices(configuration)
    .ConfigurePipeline();

app.Run();

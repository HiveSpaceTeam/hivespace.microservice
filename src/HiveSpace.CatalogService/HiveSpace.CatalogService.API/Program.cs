using HiveSpace.CatalogService.API.Extentions;
using Microsoft.Extensions.Configuration;
using HiveSpace.CatalogService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;

var catalogDbConnectionString = configuration.GetConnectionString("CatalogDb");
builder.Services.AddCatalogInfrastructure(catalogDbConnectionString);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationServices(configuration);

var app = builder.Build();


app.UseHttpsRedirection();
//app.UseCors("_myAllowSpecificOrigins");
app.UseAuthorization();

app.MapControllers();

app.Run();

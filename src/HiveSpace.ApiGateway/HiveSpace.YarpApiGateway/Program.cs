using HiveSpace.YarpApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);
var app = await builder.ConfigureServicesAsync();

app.ConfigurePipeline();
app.Run();

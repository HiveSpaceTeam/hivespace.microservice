using HiveSpace.MediaService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var app = builder
    .ConfigureServices(configuration)
    .ConfigurePipeline();

// Helper to configure CORS for Blob Storage on startup
await app.ConfigureStorageCorsAsync();

app.Run();

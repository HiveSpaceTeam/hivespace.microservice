using HiveSpace.MediaService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddAppServices();
builder.Services.AddAppValidators();
builder.Services.AddAppDatabase(builder.Configuration);

var app = builder.Build();

app.ConfigurePipeline();

app.Run();

using HiveSpace.MediaService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAppServices();
builder.Services.AddAppMediatR();
builder.Services.AddAppDatabase(builder.Configuration);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Authentication:RequireHttpsMetadata", true);
        options.MapInboundClaims = false;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.ConfigurePipeline();

app.Run();

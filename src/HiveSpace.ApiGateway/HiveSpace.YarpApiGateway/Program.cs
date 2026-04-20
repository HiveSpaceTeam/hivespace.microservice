var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
// AllowAnyOrigin() is incompatible with credentials (required by SignalR).
// Origins are read from config so each environment can declare its own set.
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5174", "http://localhost:5173", "http://localhost:5175"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Use CORS
app.UseCors();

// Required for YARP to proxy SignalR WebSocket connections
app.UseWebSockets();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapReverseProxy();
app.Run();
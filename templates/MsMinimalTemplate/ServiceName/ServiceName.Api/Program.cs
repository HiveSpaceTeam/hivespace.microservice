using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServiceName API",
        Description = "A minimal API for ServiceName",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Health check endpoint
app.MapGet("/health", () => new
{
    Status = "Healthy",
    Service = "ServiceName",
    Timestamp = DateTime.UtcNow
})
.WithName("GetHealth")
.WithTags("Health")
.WithOpenApi();

// Sample API endpoint
app.MapGet("/api/hello", (string? name) => 
{
    var greeting = string.IsNullOrEmpty(name) ? "Hello World!" : $"Hello {name}!";
    return new { 
        Message = greeting, 
        Service = "ServiceName",
        Timestamp = DateTime.UtcNow 
    };
})
.WithName("SayHello")
.WithTags("Sample")
.WithOpenApi();

// Sample POST endpoint
app.MapPost("/api/items", (Item item) => 
{
    item.Id = Guid.NewGuid();
    item.CreatedAt = DateTime.UtcNow;
    return Results.Created($"/api/items/{item.Id}", item);
})
.WithName("CreateItem")
.WithTags("Sample")
.WithOpenApi();

app.Run();

// Sample model
public record Item(string Name, string Description)
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
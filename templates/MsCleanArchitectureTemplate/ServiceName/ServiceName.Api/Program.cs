// ServiceName API - Developed by Org
// This is the main entry point for the ServiceName microservice

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "ServiceName API", 
        Version = "v1",
        Description = "ServiceName microservice developed by Org"
    });
});

// TODO: Add your dependency injection registrations here
// Example: builder.Services.AddScoped<IRepository<T>, Repository<T>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceName API v1 - Org"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
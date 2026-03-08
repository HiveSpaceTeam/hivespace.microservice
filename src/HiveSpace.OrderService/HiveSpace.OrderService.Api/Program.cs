using HiveSpace.OrderService.Application.Coupons.Commands.CreateCoupon;
using HiveSpace.OrderService.Api.Endpoints;
using HiveSpace.OrderService.Infrastructure;
using Microsoft.Extensions.Configuration;
using HiveSpace.Core;
using HiveSpace.Core.Middlewares;
using HiveSpace.Core.Filters;

// HiveSpace.OrderService API - Developed by Org
// This is the main entry point for the HiveSpace.OrderService microservice

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomExceptionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "HiveSpace.OrderService API", 
        Version = "v1",
        Description = "HiveSpace.OrderService microservice developed by Org"
    });
});

// Add Infrastructure dependencies
builder.Services.AddOrderDbContext(builder.Configuration);

// Add Core Services
builder.Services.AddCoreServices();

// Setup Authentication from AppSettings
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Authentication:RequireHttpsMetadata", true);
    });
builder.Services.AddAuthorization();

// Add MediatR from Application layer
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<CreateCouponCommand>();
});

var app = builder.Build();

try 
{
    Console.WriteLine("Starting Order Service...");
    
    // this seeding is only for the template to bootstrap the DB and users.
    // in production you will likely want a different approach.
    if (app.Environment.IsDevelopment())
    {
         Console.WriteLine("Attempting to run database migrations...");
         await SeedData.EnsureSeedDataAsync(app);
         Console.WriteLine("Database migrations completed.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unhandled exception during data seeding: {ex}");
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HiveSpace.OrderService API v1 - Org"));
}

app.UseHttpsRedirection();

// Use Core Middlewares
app.UseMiddleware<RequestIdMiddleware>();
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception != null)
        {
            var errorResponse = HiveSpace.Core.Helpers.ExceptionResponseFactory.CreateResponse(exception);
            
            context.Response.StatusCode = int.Parse(errorResponse.Status);
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    });
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapCouponEndpoints();

await app.RunAsync();
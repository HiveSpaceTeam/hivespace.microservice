using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HiveSpace.MediaService.Func.Infrastructure.Data;
using HiveSpace.MediaService.Func.Infrastructure.Storage;
using HiveSpace.MediaService.Func.Core.Interfaces;
using HiveSpace.MediaService.Func.Core.Services;
using HiveSpace.MediaService.Func.Core.Configuration;
using FluentValidation;
using HiveSpace.MediaService.Func.Core.Validators;

using HiveSpace.Core.Filters;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => 
    {
        worker.UseMiddleware<GlobalFunctionExceptionMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<System.Text.Json.JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        });

        var configuration = context.Configuration;
        var baseConnectionString = configuration["Database:MediaServiceDb"];
        
        // Add connection timeout to handle Azure SQL cold starts
        var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnectionString)
        {
            ConnectTimeout = 60, // Increase from default 30 seconds to 60 seconds
            ConnectRetryCount = 3, // Retry connection attempts
            ConnectRetryInterval = 10, // Wait 10 seconds between retries
            Pooling = true, // Enable connection pooling
            MinPoolSize = 0,
            MaxPoolSize = 100
        };
        var connectionString = connectionStringBuilder.ConnectionString;

        // Register Configuration
        services.AddSingleton<StorageConfiguration>();

        // Register Core Services
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddScoped<IQueueService, AzureQueueService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IMediaCleanupService, MediaCleanupService>();

        // Register Validators
        services.AddValidatorsFromAssemblyContaining<PresignUrlRequestValidator>();
        
        // Register Database with enhanced retry logic for Azure SQL
        services.AddDbContext<MediaDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)
                .CommandTimeout(120)); // Increase command timeout to 120 seconds for first-time operations
        });

    })
    .Build();

// Apply pending migrations automatically (Development only)
if (host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
{
    using var scope = host.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<MediaDbContext>();

    try
    {
        // Check for pending migrations before applying them
        var pendingMigrations = context.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                pendingMigrations.Count(),
                string.Join(", ", pendingMigrations));

            logger.LogInformation("Applying pending migrations...");
            context.Database.Migrate();
            logger.LogInformation("Migrations applied successfully");
        }
        else
        {
            logger.LogInformation("No pending migrations found. Database is up to date.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

host.Run();

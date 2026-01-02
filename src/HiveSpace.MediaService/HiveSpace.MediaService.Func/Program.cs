using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HiveSpace.MediaService.Func.Infrastructure.Data;
using HiveSpace.MediaService.Func.Infrastructure.Storage;
using HiveSpace.MediaService.Func.Core.Interfaces;
using HiveSpace.MediaService.Func.Core.Services;
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
        });

        var configuration = context.Configuration;
        var connectionString = configuration["Database:MediaServiceDb"];


        // Register Core Services
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddScoped<IQueueService, AzureQueueService>();
        services.AddScoped<IMediaService, MediaService>();

        // Register Validators
        services.AddValidatorsFromAssemblyContaining<PresignUrlRequestValidator>();
        
        // Register Database
        services.AddDbContext<MediaDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

    })
    .Build();

host.Run();

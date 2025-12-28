using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HiveSpace.MediaService.Core.Data;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Services;
using HiveSpace.Infrastructure.Persistence;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var connectionString = configuration["Database:MediaServiceDb"];


        // Register Core Services
        services.AddScoped<IStorageService, AzureBlobStorageService>();
        services.AddScoped<IQueueService, AzureQueueService>();
        
        // Register Database
        services.AddDbContext<MediaDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
        });

    })
    .Build();

host.Run();

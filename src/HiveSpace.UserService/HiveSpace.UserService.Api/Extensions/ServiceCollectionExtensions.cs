using Asp.Versioning;
using HiveSpace.Core;
using HiveSpace.UserService.Api.Services.Localization;
using HiveSpace.UserService.Application;
using HiveSpace.UserService.Application.Services;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Domain.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using HiveSpace.UserService.Api.Middleware;
using Microsoft.Extensions.Hosting;


namespace HiveSpace.UserService.Api.Extensions;

// --- Extension methods for separation of concerns ---

internal static class ServiceCollectionExtensions
{
    public static void AddAppApiControllers(this IServiceCollection services)
    {
        services.AddHiveSpaceControllers();

        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });
    }

    public static void AddAppApplicationServices(this IServiceCollection services)
    {
        services.AddApplication();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IUserService, Application.Services.UserService>();
        services.AddScoped<IUserAddressService, UserAddressService>();
    }

    public static void AddAppDomainServices(this IServiceCollection services)
    {
        // Domain services
        services.AddScoped<StoreManager>();
    }

    public static void AddLocalizationServices(this IServiceCollection services)
    {
        // Add localization support
        services.AddLocalization();

        // Register custom localization service
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // Configure request localization options
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = CultureMiddleware.GetSupportedCultures();
            var defaultCulture = CultureMiddleware.GetDefaultCulture();

            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
        });
    }

    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDefaultAuthentication(configuration, "user.fullaccess");
    }

    public static void AddAppAuthorization(this IServiceCollection services)
    {
        // Authorization policies are registered by AddHiveSpaceJwtBearerAuthentication.
    }

    public static void AddAppApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
    }

    public static void AddAppOpenApi(this IServiceCollection services)
    {
        services.AddDefaultOpenApi(
            "HiveSpace User Service API",
            "API for managing user profiles, addresses, settings, and stores in the HiveSpace platform");

        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });
    }
}

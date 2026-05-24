using HiveSpace.Core.OpenApi;
using HiveSpace.IdentityService.Api.Consumers;
using HiveSpace.IdentityService.Api.Configs;
using HiveSpace.IdentityService.Api.Middleware;
using HiveSpace.IdentityService.Api.Services;
using HiveSpace.IdentityService.Api.Services.Localization;
using HiveSpace.IdentityService.Core.Extensions;
using HiveSpace.IdentityService.Core.Identity;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Messaging;
using HiveSpace.IdentityService.Core.Persistence;
using HiveSpace.Infrastructure.Authorization.Extensions;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HiveSpace.IdentityService.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppOpenApi(this IServiceCollection services)
        => services.AddHiveSpaceSwaggerGen("HiveSpace IdentityService API", "HiveSpace IdentityService microservice");

    public static IServiceCollection AddAppRazorUi(this IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        return services;
    }

    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = CultureMiddleware.GetSupportedCultures();
            var defaultCulture = CultureMiddleware.GetDefaultCulture();

            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
        });

        return services;
    }

    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        return services;
    }

    public static IServiceCollection AddAppIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentityServer(options =>
            {
                options.LicenseKey = configuration.GetValue("Duende:LicenseKey", "");
                options.IssuerUri = configuration.GetValue("Issuer", "");
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.Endpoints.EnableCheckSessionEndpoint = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryClients(Config.GetClients(configuration))
            .AddAspNetIdentity<ApplicationUser>()
            .AddLicenseSummary();

        services.AddScoped<Duende.IdentityServer.Services.IProfileService, CustomProfileService>();

        return services;
    }

    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
        => services.AddHiveSpaceJwtBearerAuthentication(configuration, "identity.fullaccess");

    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddCoreServices();
        services.AddScoped<IIdentityEventPublisher, IdentityEventPublisher>();

        return services;
    }

    public static IServiceCollection AddAppMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>();
        if (messagingOptions?.EnableRabbitMq != true)
            return services;

        services.AddMassTransitWithRabbitMq<IdentityDbContext>(configuration, cfg =>
        {
            cfg.AddConsumer<StoreCreatedConsumer>()
                .Endpoint(e => e.Name = "identity-store-created");
        });

        return services;
    }
}

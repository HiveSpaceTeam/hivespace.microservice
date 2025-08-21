using Duende.IdentityServer;
using FluentValidation;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;
using HiveSpace.IdentityService.Api.Configs;
using HiveSpace.IdentityService.Application.Validators.Address;
using HiveSpace.IdentityService.Application.Validators.User;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.IdentityService.Infrastructure;
using HiveSpace.IdentityService.Infrastructure.Data;
using HiveSpace.IdentityService.Infrastructure.Repositories;
using HiveSpace.Infrastructure.Messaging.Interfaces;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Persistence.Outbox;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using HiveSpace.IdentityService.Application.Models.Requests;

namespace HiveSpace.IdentityService.Api.Extensions;

// --- Extension methods for separation of concerns ---

internal static class ServiceCollectionExtensions
{
    public static void AddAppApiControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<CustomExceptionFilter>();
        });
    }

    public static void AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(HostingExtensions.IdentityServiceDbConnection)
            ?? throw new InvalidOperationException($"Connection string '{HostingExtensions.IdentityServiceDbConnection}' not found.");
        
        services.AddAppInterceptors();
        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
        {
            var interceptors = serviceProvider.GetServices<ISaveChangesInterceptor>();
            options.UseSqlServer(connectionString)
                .AddInterceptors(interceptors);     
        });

        // Register the generic DbContext to resolve to IdentityDbContext
        // This is needed for services that depend on the generic DbContext type
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<IdentityDbContext>());

        // Add persistence infrastructure with specific DbContext type
        services.AddPersistenceInfrastructure<IdentityDbContext>();

        // Add specific outbox repository for IdentityDbContext (for background services)
        services.AddOutboxServices<IdentityDbContext>();
    }
     
    public static void AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();
    }

    // Split AddAppDependencies into two methods for better separation of concerns
    public static void AddAppInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventMapper, IdentityIntegrationEventMapper>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserRepository, UserRepository>();
    }

    /// <summary>
    /// Registers application-level services required by the Identity API.
    /// </summary>
    /// <remarks>
    /// Adds a scoped registration for <see cref="IUserContext"/> with <see cref="UserContext"/> as the implementation.
    /// </remarks>
    public static void AddAppApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
    }

    /// <summary>
    /// Registers FluentValidation validators for identity-related request DTOs in the DI container.
    /// </summary>
    /// <remarks>
    /// Adds transient registrations for:
    /// - <see cref="AddressRequestDto"/> -> <see cref="AddressValidator"/>,
    /// - <see cref="SignupRequestDto"/> -> <see cref="SignupValidator"/>,
    /// - <see cref="UpdateUserRequestDto"/> -> <see cref="UpdateUserValidator"/>,
    /// - <see cref="ChangePasswordRequestDto"/> -> <see cref="ChangePasswordValidator"/>.
    /// These validators are resolved as <see cref="IValidator{T}"/> instances.
    /// </remarks>
    public static void AddFluentValidationServices(this IServiceCollection services)
    {
        services.AddTransient<IValidator<AddressRequestDto>, AddressValidator>();
        services.AddTransient<IValidator<SignupRequestDto>, SignupValidator>();
        services.AddTransient<IValidator<UpdateUserRequestDto>, UpdateUserValidator>();
        services.AddTransient<IValidator<ChangePasswordRequestDto>, ChangePasswordValidator>();
    }

    public static void AddAppIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentityServer(options =>
            {
                options.LicenseKey = configuration.GetValue("Duende:LicenseKey", "");
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
    }

    /// <summary>
    /// Registers authentication handlers: a Local API handler that expects the "identity.fullaccess" scope,
    /// and optional Google and Facebook external login handlers when their credentials are provided in configuration.
    /// </summary>
    /// <param name="configuration">Configuration used to read external provider credentials:
    /// "Authentication:Google:ClientId" / "Authentication:Google:ClientSecret" and
    /// "Authentication:Facebook:AppId" / "Authentication:Facebook:AppSecret".
    /// A provider is registered only if both required values for that provider are present and non-empty.</param>
    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication()
            .AddLocalApi(options =>
            {
                options.ExpectedScope = "identity.fullaccess";
            });

        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
        {
            services
                .AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.CallbackPath = "/signin-google";
                });
        }

        var facebookAppId = configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
        {
            services
                .AddAuthentication()
                .AddFacebook(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.AppId = facebookAppId;
                    options.AppSecret = facebookAppSecret;
                    options.CallbackPath = "/signin-facebook";
                });
        }
    }

    /// <summary>
    /// Registers the application's authorization policy that requires an authenticated caller to have the "identity.fullaccess" scope using the IdentityServer Local API authentication scheme.
    /// </summary>
    /// <remarks>
    /// Adds a policy named "RequireIdentityFullAccessScope" which:
    /// - requires an authenticated user,
    /// - requires a "scope" claim with value "identity.fullaccess",
    /// - and restricts evaluation to the IdentityServer local API authentication scheme.
    /// </remarks>
    public static void AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireIdentityFullAccessScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "identity.fullaccess");
                policy.AuthenticationSchemes.Add(IdentityServerConstants.LocalApi.AuthenticationScheme);
            });
        });
    }

    public static void AddAppApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });
    }


}

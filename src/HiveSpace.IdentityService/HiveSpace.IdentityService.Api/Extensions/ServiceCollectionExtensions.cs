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
    /// Adds a scoped registration for IUserContext -> UserContext so a user context is available per request.
    /// </remarks>
    public static void AddAppApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
    }

    /// <summary>
    /// Registers FluentValidation validators used by the API as transient services in the DI container.
    /// </summary>
    /// <remarks>
    /// Registers the following validators:
    /// - <see cref="IValidator{AddressRequestDto}"/> -> <see cref="AddressValidator"/>
    /// - <see cref="IValidator{SignupRequestDto}"/> -> <see cref="SignupValidator"/>
    /// - <see cref="IValidator{UpdateUserRequestDto}"/> -> <see cref="UpdateUserValidator"/>
    /// - <see cref="IValidator{ChangePasswordRequestDto}"/> -> <see cref="ChangePasswordValidator"/>
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
    /// Registers authentication handlers for the application:
    /// - Local API authentication expecting the "identity.fullaccess" scope.
    /// - Optional external Google authentication when <c>Authentication:Google:ClientId</c> and <c>Authentication:Google:ClientSecret</c> are present in configuration.
    /// - Optional external Facebook authentication when <c>Authentication:Facebook:AppId</c> and <c>Authentication:Facebook:AppSecret</c> are present in configuration.
    /// </summary>
    /// <param name="configuration">Configuration containing external provider credentials. Recognized keys:
    /// <c>Authentication:Google:ClientId</c>, <c>Authentication:Google:ClientSecret</c>,
    /// <c>Authentication:Facebook:AppId</c>, <c>Authentication:Facebook:AppSecret</c>.
    /// </param>
    /// <remarks>
    /// When Google is enabled the external callback path is <c>/signin-google</c>; when Facebook is enabled the callback path is <c>/signin-facebook</c>.
    /// Both external providers use the IdentityServer external cookie sign-in scheme.
    /// </remarks>
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
    /// Registers authorization policies for the application.
    /// </summary>
    /// <remarks>
    /// Adds the "RequireIdentityFullAccessScope" policy which requires an authenticated user,
    /// a "scope" claim containing "identity.fullaccess", and authentication via the IdentityServer
    /// local API authentication scheme.
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

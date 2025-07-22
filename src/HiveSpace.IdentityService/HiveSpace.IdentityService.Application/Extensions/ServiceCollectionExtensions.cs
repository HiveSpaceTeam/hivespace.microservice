using Duende.IdentityServer;
using FluentValidation;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;
using HiveSpace.IdentityService.Application.Configs;
using HiveSpace.IdentityService.Application.Interfaces;
using HiveSpace.IdentityService.Application.Models.Requests;
using HiveSpace.IdentityService.Application.Services;
using HiveSpace.IdentityService.Application.Validators.Address;
using HiveSpace.IdentityService.Application.Validators.User;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.IdentityService.Infrastructure.Data;
using HiveSpace.IdentityService.Infrastructure.Repositories;
using HiveSpace.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.IdentityService.Application.Extensions;

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
        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddSingleton<AuditableInterceptor>();
        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
            options.UseSqlServer(connectionString)
                   .AddInterceptors(
                        serviceProvider.GetRequiredService<SoftDeleteInterceptor>(),
                        serviceProvider.GetRequiredService<AuditableInterceptor>()
                    ));
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
        services.AddHttpContextAccessor();
        services.AddScoped<IUserRepository, UserRepository>();
    }

    public static void AddAppApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IUserService, UserService>();
    }

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
                options.LicenseKey = configuration.GetValue<string>("Duende:LicenseKey", "");
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
    }

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

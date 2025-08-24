using Duende.IdentityServer;
using FluentValidation;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;
using HiveSpace.UserService.Api.Configs;
using HiveSpace.UserService.Infrastructure.Identity;
// using HiveSpace.UserService.Application.Validators.Address;
// using HiveSpace.UserService.Application.Validators.User;
using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.Infrastructure.Messaging.Interfaces;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Persistence.Outbox;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
// using HiveSpace.UserService.Application.Models.Requests;

namespace HiveSpace.UserService.Api.Extensions;

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
     
    public static void AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();
    }

    // Split AddAppDependencies into two methods for better separation of concerns
    public static void AddAppInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
    }

    public static void AddAppApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
    }

    public static void AddFluentValidationServices(this IServiceCollection services)
    {
        // TODO: Uncomment when Application layer is created
        // services.AddTransient<IValidator<AddressRequestDto>, AddressValidator>();
        // services.AddTransient<IValidator<SignupRequestDto>, SignupValidator>();
        // services.AddTransient<IValidator<UpdateUserRequestDto>, UpdateUserValidator>();
        // services.AddTransient<IValidator<ChangePasswordRequestDto>, ChangePasswordValidator>();
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

    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication()
            .AddLocalApi(options =>
            {
                options.ExpectedScope = "user.fullaccess";
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

        // TODO: Add Facebook authentication when package is available
        // var facebookAppId = configuration["Authentication:Facebook:AppId"];
        // var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"];
        // if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
        // {
        //     services
        //         .AddAuthentication()
        //         .AddFacebook(options =>
        //         {
        //             options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        //             options.AppId = facebookAppId;
        //             options.AppSecret = facebookAppSecret;
        //             options.CallbackPath = "/signin-facebook";
        //         });
        // }
    }

    public static void AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireUserFullAccessScope", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "user.fullaccess");
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

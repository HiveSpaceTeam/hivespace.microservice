using Duende.IdentityServer;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;
using HiveSpace.UserService.Api.Configs;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using HiveSpace.UserService.Application.Services;
using HiveSpace.UserService.Application.Interfaces;

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
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

        // Configure the application cookie so it's available during OIDC/OAuth redirects
        // (cross-site) from external providers like Google. Setting SameSite=None and
        // SecurePolicy=Always ensures browsers will send the cookie on the redirect back
        // to our site. Adjust paths and other options as needed for your environment.
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    }

    // Split AddAppDependencies into two methods for better separation of concerns
    public static void AddAppInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
    }

    public static void AddAppApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IAdminService, AdminService>();
    }

    public static void AddAppDomainServices(this IServiceCollection services)
    {
        // Domain services
        services.AddScoped<StoreManager>();
        services.AddScoped<UserManager>();
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

        // Register the custom profile service to enrich tokens with application-specific claims
        // Ensure the implementation type `CustomProfileService` is available from
        // `HiveSpace.UserService.Infrastructure.Identity` namespace/project.
        services.AddScoped<Duende.IdentityServer.Services.IProfileService, CustomProfileService>();
    }

    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(IdentityServerConstants.LocalApi.AuthenticationScheme)
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
                    // Ensure the correlation cookie used by the Google handler is sent on the
                    // cross-site callback. Without this, some browsers will drop the cookie
                    // causing "Correlation failed." errors.
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    // Handle remote failures (for example when correlation cookie is missing)
                    // and redirect back to the login page with a friendly error so the UI
                    // can render it instead of throwing an unhandled exception.
                    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                    {
                        OnRemoteFailure = ctx =>
                        {
                            try
                            {
                                var failure = ctx.Failure?.Message ?? "External authentication failed";
                                string? returnUrl = null;
                                if (ctx.Properties?.Items != null && ctx.Properties.Items.TryGetValue("returnUrl", out var ru))
                                    returnUrl = ru;

                                var redirect = "/Account/Login?error=" + System.Uri.EscapeDataString(failure);
                                if (!string.IsNullOrEmpty(returnUrl))
                                {
                                    redirect += "&returnUrl=" + System.Uri.EscapeDataString(returnUrl);
                                }

                                ctx.Response.Redirect(redirect);
                                ctx.HandleResponse();
                            }
                            catch
                            {
                                // If anything goes wrong here, just let the default behavior run.
                            }

                            return System.Threading.Tasks.Task.CompletedTask;
                        }
                    };
                });
        }
        
        // Configure the external cookie scheme used by IdentityServer for temporary
        // storage of external identity information during the external authentication
        // roundtrip. Make sure SameSite=None so the cookie survives the cross-site
        // redirect back from the provider.
        services.Configure<CookieAuthenticationOptions>(IdentityServerConstants.ExternalCookieAuthenticationScheme, options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Path = "/";
        });
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

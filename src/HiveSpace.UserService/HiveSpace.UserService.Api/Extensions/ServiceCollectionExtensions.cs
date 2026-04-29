using Asp.Versioning;
using Duende.IdentityServer;
using HiveSpace.Core;
using HiveSpace.Core.OpenApi;
using HiveSpace.Infrastructure.Authorization.Extensions;
using HiveSpace.UserService.Api.Configs;
using HiveSpace.UserService.Api.Services.Localization;
using HiveSpace.UserService.Application;
using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Services;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using HiveSpace.UserService.Application.Services;
using HiveSpace.UserService.Application.Interfaces.Services;
using HiveSpace.UserService.Infrastructure.Settings;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using HiveSpace.UserService.Api.Middleware;


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

    public static void AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<UserDbContext>()
        .AddUserStore<CustomUserStore>() // Use our custom UserStore for direct role storage
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

    public static void AddAppApplicationServices(this IServiceCollection services)
    {
        services.AddApplication();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IUserService, Application.Services.UserService>();
        services.AddScoped<IUserAddressService, UserAddressService>();
    }

    public static void AddAppDomainServices(this IServiceCollection services)
    {
        // Domain services
        services.AddScoped<StoreManager>();
        services.AddScoped<UserManager>();
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

    public static void AddEmailConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure EmailSettings
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        // Configure FluentEmail with SMTP
        services.AddFluentEmail(configuration["EmailSettings:FromEmail"], configuration["EmailSettings:FromName"])
            .AddRazorRenderer()
            .AddSmtpSender(
                configuration["EmailSettings:SmtpServer"],
                int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587"),
                configuration["EmailSettings:SmtpUser"],
                configuration["EmailSettings:SmtpPassword"]
            );
    }

    public static void AddAppIdentityServer(this IServiceCollection services, IConfiguration configuration)
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
    }

    public static void AddAppAuthorization(this IServiceCollection services)
    {
        // Use shared HiveSpace authorization with LocalApi only (User Service hosts IdentityServer)
        services.AddHiveSpaceAuthorizationForLocalApi("user.fullaccess");
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
        services.AddHiveSpaceOpenApi(
            "HiveSpace User Service API",
            "API for managing users, authentication, and authorization in the HiveSpace platform");

        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });
    }
}

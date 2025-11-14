using Duende.IdentityServer;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;
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
using Microsoft.OpenApi.Models;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using HiveSpace.UserService.Api.Middleware;


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

        // Configure global route prefix using built-in .NET support
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
        // Add MediatR and register handlers
        services.AddApplication();

        // Register application services
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IUserService, Application.Services.UserService>();
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
        // Use shared HiveSpace authorization with LocalApi only (User Service hosts IdentityServer)
        services.AddHiveSpaceAuthorizationForLocalApi("user.fullaccess");
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

    public static void AddAppSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Basic API Info
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HiveSpace User Service API",
                Version = "v1.0",
                Description = "API for managing users, authentication, and authorization in the HiveSpace platform",
                Contact = new OpenApiContact
                {
                    Name = "HiveSpace Team",
                    Email = "support@hivespace.com"
                }
            });

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token in the format: Bearer {your token}"
            });

            // Apply security requirements globally
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML documentation (if available)
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });
    }
}

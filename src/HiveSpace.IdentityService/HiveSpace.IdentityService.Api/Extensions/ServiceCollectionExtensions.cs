using HiveSpace.IdentityService.Api.Consumers;
using HiveSpace.IdentityService.Api.Configs;
using HiveSpace.IdentityService.Api.Middleware;
using HiveSpace.IdentityService.Api.Services;
using HiveSpace.IdentityService.Core.Extensions;
using HiveSpace.IdentityService.Core.DomainModels;
using HiveSpace.IdentityService.Core.Exceptions;
using HiveSpace.IdentityService.Core.Features.AccountSessions.Services;
using HiveSpace.IdentityService.Core.Interfaces.Messaging;
using HiveSpace.IdentityService.Core.Interfaces.Services;
using HiveSpace.IdentityService.Core.Messaging;
using HiveSpace.IdentityService.Core.Persistence;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace HiveSpace.IdentityService.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppOpenApi(this IServiceCollection services)
        => services.AddDefaultOpenApi("HiveSpace IdentityService API", "HiveSpace IdentityService microservice");

    public static IServiceCollection AddAppForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static IServiceCollection AddAppSessionState(this IServiceCollection services)
    {
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

        services.ConfigureExternalCookie(options =>
        {
            ConfigureGoogleExternalCookie(options);
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
    {
        services.AddDefaultAuthentication(configuration, "identity.fullaccess");
        services.Configure<GoogleExternalAuthOptions>(configuration.GetSection(GoogleExternalAuthOptions.SectionName));
        services.PostConfigure<GoogleExternalAuthOptions>(options =>
        {
            if (options.AllowedFrontendOrigins.Count == 0)
            {
                AddFrontendOrigin(configuration, options, "buyer");
                AddFrontendOrigin(configuration, options, "seller");
            }

            if (!options.IsConfigured())
                throw new ConfigurationException([new Error(IdentityDomainErrorCode.InvalidConfiguration, GoogleExternalAuthOptions.SectionName)]);
        });

        services.AddAuthentication()
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                var googleOptions = configuration.GetSection(GoogleExternalAuthOptions.SectionName).Get<GoogleExternalAuthOptions>()
                    ?? new GoogleExternalAuthOptions();
                if (googleOptions.AllowedFrontendOrigins.Count == 0)
                {
                    AddFrontendOrigin(configuration, googleOptions, "buyer");
                    AddFrontendOrigin(configuration, googleOptions, "seller");
                }

                if (!googleOptions.IsConfigured())
                    throw new ConfigurationException([new Error(IdentityDomainErrorCode.InvalidConfiguration, GoogleExternalAuthOptions.SectionName)]);

                options.ClientId = googleOptions.ClientId;
                options.ClientSecret = googleOptions.ClientSecret;
                options.CallbackPath = googleOptions.CallbackPath;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.SaveTokens = false;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.CorrelationCookie.IsEssential = true;
                options.Scope.Add("email");
                options.Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = context =>
                    {
                        context.Response.Redirect(BuildGoogleAuthorizationRedirectUri(context.RedirectUri, googleOptions));
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect(BuildGoogleRemoteFailureRedirect(configuration, context.Request.Query));
                        return Task.CompletedTask;
                    },
                    OnCreatingTicket = context =>
                    {
                        if (context.User.TryGetProperty("email_verified", out var emailVerified))
                            context.Identity?.AddClaim(new Claim("email_verified", emailVerified.ToString()));

                        return Task.CompletedTask;
                    }
                };
            });

        services.PostConfigure<CookieAuthenticationOptions>(
            IdentityConstants.ExternalScheme,
            ConfigureGoogleExternalCookie);

        return services;
    }

    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("IdentityDb")));

        return services;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddCoreServices();
        services.AddHttpContextAccessor();
        services.AddScoped<IIdentityEventPublisher, IdentityEventPublisher>();
        services.AddScoped<ITokenCookieService, TokenCookieService>();
        services.AddScoped<ICsrfTokenService, CsrfTokenService>();
        services.AddScoped<IPendingGoogleLinkStore, PendingGoogleLinkCookieStore>();
        services.AddScoped<IAccountSessionIssuer, AccountSessionIssuer>();

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

    private static void AddFrontendOrigin(IConfiguration configuration, GoogleExternalAuthOptions options, string app)
    {
        var origin = configuration[$"FrontendRedirects:{app}:Origin"];
        if (!string.IsNullOrWhiteSpace(origin))
            options.AllowedFrontendOrigins[app] = origin;
    }

    private static void ConfigureGoogleExternalCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Path = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    }

    private static string BuildGoogleAuthorizationRedirectUri(string authorizationEndpoint, GoogleExternalAuthOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PublicCallbackOrigin))
            return authorizationEndpoint;

        var uri = new Uri(authorizationEndpoint);
        var query = QueryHelpers.ParseQuery(uri.Query)
            .ToDictionary(
                pair => pair.Key,
                pair => (string?)pair.Value.ToString(),
                StringComparer.OrdinalIgnoreCase);

        query["redirect_uri"] = BuildGooglePublicCallbackUri(options);

        var builder = new UriBuilder(uri)
        {
            Query = QueryString.Create(query).Value?.TrimStart('?') ?? string.Empty
        };

        return builder.Uri.ToString();
    }

    private static string BuildGooglePublicCallbackUri(GoogleExternalAuthOptions options)
    {
        var origin = new Uri(options.PublicCallbackOrigin!.TrimEnd('/') + "/");
        return new Uri(origin, options.CallbackPath.TrimStart('/')).ToString();
    }

    private static string BuildGoogleRemoteFailureRedirect(IConfiguration configuration, IQueryCollection query)
    {
        var app = query.TryGetValue("app", out var appValue)
            ? appValue.ToString().Trim().ToLowerInvariant()
            : "buyer";
        if (app is not "buyer" and not "seller")
            app = "buyer";

        var origin = configuration[$"FrontendRedirects:{app}:Origin"]
            ?? configuration["DefaultRedirectUrl"]
            ?? "http://localhost:5175";
        var path = configuration[$"FrontendRedirects:{app}:loginPath"] ?? "/signin";

        var builder = new UriBuilder(new Uri(new Uri(origin.TrimEnd('/')), path.TrimStart('/')));
        var queryValues = new Dictionary<string, string?>
        {
            ["error"] = "GoogleFailed"
        };

        if (query.TryGetValue("returnUrl", out var returnUrl) && !string.IsNullOrWhiteSpace(returnUrl))
            queryValues["returnUrl"] = returnUrl.ToString();

        if (query.TryGetValue("culture", out var culture) && !string.IsNullOrWhiteSpace(culture))
            queryValues["culture"] = culture.ToString();

        builder.Query = QueryString.Create(queryValues).Value?.TrimStart('?') ?? string.Empty;
        return builder.Uri.ToString();
    }
}

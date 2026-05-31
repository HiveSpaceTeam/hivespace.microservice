using HiveSpace.YarpApiGateway.Middleware;
using System.Text.Json;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
// AllowAnyOrigin() is incompatible with credentials (required by SignalR).
// Origins are read from config so each environment can declare its own set.
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5174", "http://localhost:5173", "http://localhost:5175"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var validAudiences = builder.Configuration.GetSection("Authentication:ValidAudiences").Get<string[]>()
    ?? builder.Configuration.GetSection("BrowserSession:AccessTokenAudiences").Get<string[]>()
    ?? [];
var authority = builder.Configuration["Authentication:Authority"];
var requireHttpsMetadata = builder.Configuration.GetValue("Authentication:RequireHttpsMetadata", true);
var metadataAddress = string.IsNullOrWhiteSpace(authority)
    ? null
    : $"{authority.TrimEnd('/')}/.well-known/openid-configuration";
var identityConfiguration = metadataAddress is null
    ? null
    : await OpenIdConnectConfigurationRetriever.GetAsync(
        metadataAddress,
        new HttpDocumentRetriever { RequireHttps = requireHttpsMetadata },
        CancellationToken.None);
ICollection<SecurityKey> issuerSigningKeys = identityConfiguration?.SigningKeys.ToArray() ?? [];

if (issuerSigningKeys.Count == 0 && metadataAddress is not null)
{
    using var httpClient = new HttpClient();
    var metadataJson = await httpClient.GetStringAsync(metadataAddress);
    using var metadataDocument = JsonDocument.Parse(metadataJson);
    var jwksUri = metadataDocument.RootElement.GetProperty("jwks_uri").GetString();
    if (string.IsNullOrWhiteSpace(jwksUri))
        throw new InvalidOperationException("Identity metadata does not declare jwks_uri.");

    var jwksJson = await httpClient.GetStringAsync(jwksUri);
    issuerSigningKeys = new JsonWebKeySet(jwksJson).Keys.Cast<SecurityKey>().ToArray();
}

if (identityConfiguration is not null && identityConfiguration.SigningKeys.Count == 0)
{
    foreach (var signingKey in issuerSigningKeys)
        identityConfiguration.SigningKeys.Add(signingKey);
}

builder.Services.AddSingleton(new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = identityConfiguration?.Issuer ?? authority,
    ValidateAudience = validAudiences.Length > 0,
    ValidAudiences = validAudiences,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    IssuerSigningKeys = issuerSigningKeys,
    NameClaimType = "name",
    RoleClaimType = "role",
    ClockSkew = TimeSpan.FromMinutes(1)
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Use CORS
app.UseCors();

// Required for YARP to proxy SignalR WebSocket connections
app.UseWebSockets();

app.UseMiddleware<CsrfValidationMiddleware>();
app.UseMiddleware<SessionForwardingMiddleware>();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapReverseProxy();
app.Run();

# Startup File Conventions

## File Roles

| File | Role |
|------|------|
| `Program.cs` | Entry point only — calls `ConfigureServices()` + `ConfigurePipeline()` + optional dev seeding. No inline service registrations. |
| `Extensions/HostingExtensions.cs` | `ConfigureServices()` orchestrates `AddApp*()` calls; `ConfigurePipeline()` builds the middleware stack. |
| `Extensions/ServiceCollectionExtensions.cs` | `AddApp*()` thin wrappers — delegate to shared lib helpers, add service-specific extras only. Never re-implement what a shared helper already does. |

## Shared Startup Helpers

These helpers live in shared libs and must be used instead of re-implementing the same code per service:

| Method | Namespace | What it does |
|--------|-----------|--------------|
| `builder.AddDefaultSerilog()` | `Microsoft.Extensions.Hosting` | Registers the shared Serilog host configuration. Call from `ConfigureServices()` before `AddServiceDefaults()` in every included API project. |
| `builder.AddServiceDefaults()` | `Microsoft.Extensions.Hosting` | Registers Aspire/OpenTelemetry defaults, service discovery-compatible HTTP defaults, and shared health checks. Call from `ConfigureServices()` before service-specific registrations where compatible. |
| `app.MapDefaultEndpoints()` | `Microsoft.Extensions.Hosting` | Maps shared health endpoints (`/health`, and `/alive` in development). Call from `ConfigurePipeline()` without removing service-specific health endpoints that use different paths. |
| `services.AddDefaultOpenApi(title, description)` | `Microsoft.Extensions.Hosting` | Thin wrapper around `AddHiveSpaceSwaggerGen`; service wrappers keep explicit titles/descriptions. |
| `services.AddDefaultAuthentication(configuration, scope, configure?)` | `Microsoft.Extensions.Hosting` | Thin wrapper around `AddHiveSpaceJwtBearerAuthentication`; service wrappers keep explicit scopes and callbacks. Do not use for ApiGateway gateway-specific auth. |
| `services.AddHiveSpaceSwaggerGen(title, description)` | `HiveSpace.Core.OpenApi` | `AddEndpointsApiExplorer` + `AddSwaggerGen` with Bearer security definition |
| `services.AddHiveSpaceJwtBearerAuthentication(config, scope, configure?)` | `HiveSpace.Infrastructure.Authorization.Extensions` | `AddJwtBearer` + `AddHiveSpaceAuthorization(scope)` in one call. Pass the optional `configure` callback for service-specific options (e.g. SignalR token handling in NotificationService). |
| `services.AddHiveSpaceControllers()` | `HiveSpace.Core` | `AddControllers` + `CustomExceptionFilter`. Use only for `UserService` or an explicitly approved controller exception. Returns `IMvcBuilder` for chaining `.AddJsonOptions()`. |
| `app.UseHiveSpaceExceptionHandler()` | `HiveSpace.Core.Extensions` | `UseExceptionHandler` + `ExceptionResponseFactory` JSON response. Call in every service's `ConfigurePipeline`. |

## `ServiceCollectionExtensions.cs` — Thin Wrapper Pattern

```csharp
// ✅ Correct — one-liner that delegates scope to shared helper
public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    => services.AddHiveSpaceJwtBearerAuthentication(configuration, "catalog.fullaccess");

// ✅ Correct — callback for service-specific JwtBearerOptions
public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
{
    services.AddHiveSpaceJwtBearerAuthentication(configuration, "notification.fullaccess", options =>
    {
        options.Events = new JwtBearerEvents { OnMessageReceived = ... }; // SignalR token
        options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "sub" };
    });
}

// ✅ Correct — delegates base, adds Enumeration JSON converters on top
public static void AddAppApiControllers(this IServiceCollection services)
{
    services.AddHiveSpaceControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new EnumerationJsonConverterFactory());
            });
}

// ❌ Wrong — re-implements what AddHiveSpaceJwtBearerAuthentication already does
public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
{
    services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options => { /* ... */ });
    services.AddHiveSpaceAuthorization("catalog.fullaccess");
}
```

## `HostingExtensions.cs` — Canonical Pipeline

```csharp
public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
{
    builder.AddDefaultSerilog();
    builder.AddServiceDefaults();
    builder.Services.AddAppOpenApi();
    builder.Services.AddAppAuthentication(builder.Configuration);
    // service-specific AddApp* calls
    return builder.Build();
}

public static WebApplication ConfigurePipeline(this WebApplication app)
{
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.MapScalarApiReference(options => options
            .WithTitle("HiveSpace [Service] API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
    }

    app.UseHttpsRedirection();
    app.UseMiddleware<RequestIdMiddleware>();  // Full Services only
    app.UseHiveSpaceExceptionHandler();        // All services except UserService

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapDefaultEndpoints();
    app.MapXxxEndpoints();                     // default for all non-UserService services

    return app;
}
```

**Serilog rule**: API projects use the shared Serilog host setup through `builder.AddDefaultSerilog()` and request logging through `app.UseSerilogRequestLogging()`. Do not configure Serilog directly in `Program.cs`.

**UserService note**: Uses localization/culture middleware in addition to the standard pipeline shape. Still uses the shared Serilog setup and shared health endpoint mapping.

## Database Migration & Seeding

**Mandatory rule**: every service that owns a database MUST wire migration-at-startup in `Program.cs`. Place the call after `ConfigurePipeline()` and before `app.Run()`, guarded by `IsDevelopment()`.

```csharp
app.ConfigurePipeline();

if (app.Environment.IsDevelopment())
    await DataSeeder.EnsureSeedDataAsync(app);

app.Run();
```

### Standard `DataSeeder` pattern (services with seed data)

`DataSeeder.EnsureSeedDataAsync` lives in the Infrastructure project. It must:
1. Resolve the service's `DbContext` from a scoped `IServiceProvider`
2. Call `GetPendingMigrationsAsync` — apply with `MigrateAsync` only if the list is non-empty
3. Iterate registered `ISeeder` implementations ordered by `ISeeder.Order`, calling `SeedAsync` on each

```csharp
public static async Task EnsureSeedDataAsync(WebApplication app, CancellationToken cancellationToken = default)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db     = scope.ServiceProvider.GetRequiredService<TDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<TDbContext>>();

    var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
    if (pending.Count > 0)
    {
        logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
            pending.Count, string.Join(", ", pending));
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migrations applied successfully.");
    }

    var seeders = scope.ServiceProvider
        .GetRequiredService<IEnumerable<ISeeder>>()
        .OrderBy(s => s.Order);

    foreach (var seeder in seeders)
        await seeder.SeedAsync(cancellationToken);
}
```

### Migration-only variant (no seed data)

For services that have a database but no reference data to seed (e.g. MediaService), omit the `ISeeder` loop — keep only the migration block above.

### Per-service summary

| Service | DataSeeder location | Has ISeeder plugins |
|---------|--------------------|--------------------|
| UserService | `HiveSpace.UserService.Infrastructure/DataSeeder.cs` | No — seeds via private methods (Identity users, stores) |
| CatalogService | `HiveSpace.CatalogService.Infrastructure/DataSeeder.cs` | Yes |
| OrderService | `HiveSpace.OrderService.Infrastructure/DataSeeder.cs` | Yes |
| PaymentService | `HiveSpace.PaymentService.Infrastructure/DataSeeder.cs` | Yes |
| NotificationService | `HiveSpace.NotificationService.Core/Persistence/DataSeeder.cs` | Yes |
| MediaService | `HiveSpace.MediaService.Core/Infrastructure/DataSeeder.cs` | No — migration-only |

## Local Runtime Configuration

The Aspire AppHost is the preferred backend local startup flow:

```powershell
dotnet run --project .\src\HiveSpace.AppHost\HiveSpace.AppHost.csproj
```

Docker Compose is replaced for backend local development. Frontend dev servers remain outside AppHost v1 and continue using `http://localhost:5000`.

Dependency endpoints and secrets belong under compact lowercase `ConnectionStrings` keys, including service database keys, `rabbitmq`, `kafka`, `redis`, `azureservicebus`, and `azurestorage`. Broker enablement stays under `Messaging:EnableRabbitMq`, `Messaging:EnableKafka`, and `Messaging:EnableAzureServiceBus`; nested `Messaging` provider sections may contain only non-secret tuning such as outbox limits, prefetch count, client ID, consumer group, or security protocol.

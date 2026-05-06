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
| `services.AddHiveSpaceSwaggerGen(title, description)` | `HiveSpace.Core.OpenApi` | `AddEndpointsApiExplorer` + `AddSwaggerGen` with Bearer security definition |
| `services.AddHiveSpaceJwtBearerAuthentication(config, scope, configure?)` | `HiveSpace.Infrastructure.Authorization.Extensions` | `AddJwtBearer` + `AddHiveSpaceAuthorization(scope)` in one call. Pass the optional `configure` callback for service-specific options (e.g. SignalR token handling in NotificationService). |
| `services.AddHiveSpaceControllers()` | `HiveSpace.Core` | `AddControllers` + `CustomExceptionFilter`. Returns `IMvcBuilder` for chaining `.AddJsonOptions()`. |
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
public static WebApplication ConfigurePipeline(this WebApplication app)
{
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
    app.MapControllers();                      // or MapXxxEndpoints() for Minimal API services

    return app;
}
```

**UserService exception**: Keeps its own pipeline (IdentityServer, Razor Pages, Serilog, session, culture middleware). Does not call `UseHiveSpaceExceptionHandler`.

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

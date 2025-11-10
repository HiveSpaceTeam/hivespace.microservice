using System.Globalization;
using System.Text;
using Duende.IdentityServer.Licensing;
using HiveSpace.UserService.Api.Extensions;
using HiveSpace.UserService.Infrastructure;
using HiveSpace.UserService.Infrastructure.Data;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    // Configure Session
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", formatProvider: CultureInfo.InvariantCulture)
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices(configuration)
        .ConfigurePipeline();


    Log.Information("Ensuring database exists and is up to date");
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        context.Database.EnsureCreated();
    }

    // this seeding is only for the template to bootstrap the DB and users.
    // in production you will likely want a different approach.
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Seeding database...");
        SeedData.EnsureSeedData(app);

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            var usage = app.Services.GetRequiredService<LicenseUsageSummary>();
            Console.Write(Summary(usage));
            Console.ReadKey();
        });
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

// TODO: Fix license usage summary when needed
static string Summary(LicenseUsageSummary usage)
{
    var sb = new StringBuilder();
    sb.AppendLine("IdentityServer Usage Summary:");
    
    // Use string.Format with CultureInfo.InvariantCulture for compatibility
    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  License: {0}", usage?.LicenseEdition ?? "Unknown"));
    
    // Make collection accesses null-safe
    var features = (usage?.FeaturesUsed != null && usage.FeaturesUsed.Count > 0) 
        ? string.Join(", ", usage.FeaturesUsed) 
        : "None";
    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  Business and Enterprise Edition Features Used: {0}", features));
    
    var clientCount = usage?.ClientsUsed?.Count ?? 0;
    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  {0} Client Id(s) Used", clientCount));
    
    var issuerCount = usage?.IssuersUsed?.Count ?? 0;
    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "  {0} Issuer(s) Used", issuerCount));

    return sb.ToString();
}
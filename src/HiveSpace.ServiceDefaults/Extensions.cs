using HiveSpace.Core.OpenApi;
using HiveSpace.Infrastructure.Authorization.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    private const string MassTransitActivitySourceName = "MassTransit";
    private const string MassTransitMeterName = "MassTransit";

    public static WebApplicationBuilder AddDefaultSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, logger) =>
        {
            logger
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.With<ActivityLogEventEnricher>()
                .Enrich.FromLogContext()
                .Filter.ByExcluding(IsBackgroundInfrastructureLog)
                .Filter.ByExcluding(IsMassTransitOutboxQueryLog)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} TraceId={TraceId} SpanId={SpanId} RequestId={RequestId} {Message:lj}{NewLine}{Exception}",
                    formatProvider: CultureInfo.InvariantCulture);
        });

        return builder;
    }

    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddServiceDiscovery();
        });

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");

        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live")
            });
        }

        return app;
    }

    public static IHostApplicationBuilder AddDefaultAuthentication(
        this IHostApplicationBuilder builder,
        string scope,
        Action<JwtBearerOptions>? configure = null)
    {
        builder.Services.AddHiveSpaceJwtBearerAuthentication(builder.Configuration, scope, configure);
        return builder;
    }

    public static IServiceCollection AddDefaultAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string scope,
        Action<JwtBearerOptions>? configure = null)
    {
        services.AddHiveSpaceJwtBearerAuthentication(configuration, scope, configure);
        return services;
    }

    public static IServiceCollection AddDefaultOpenApi(
        this IServiceCollection services,
        string title,
        string description = "")
        => services.AddHiveSpaceSwaggerGen(title, description);

    private static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(MassTransitMeterName);

                if (useOtlpExporter)
                    metrics.AddOtlpExporter();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.Filter = command => !IsFilteredSqlQuery(command);
                    })
                    .AddSource(MassTransitActivitySourceName);

                if (useOtlpExporter)
                    tracing.AddOtlpExporter();
            });
    }

    private static bool IsMassTransitOutboxQueryLog(LogEvent logEvent)
    {
        if (!TryGetSourceContext(logEvent, out var sourceContext))
            return false;

        if (!sourceContext.Contains("Microsoft.EntityFrameworkCore.Database.Command", StringComparison.Ordinal)
            && !sourceContext.Contains("MassTransit.EntityFrameworkCoreIntegration", StringComparison.Ordinal))
        {
            return false;
        }

        var renderedMessage = logEvent.RenderMessage(CultureInfo.InvariantCulture);
        return IsMassTransitOutboxQuery(renderedMessage);
    }

    private static bool IsBackgroundInfrastructureLog(LogEvent logEvent)
    {
        if (!TryGetSourceContext(logEvent, out var sourceContext))
            return false;

        if (sourceContext.StartsWith("Hangfire.", StringComparison.Ordinal)
            || sourceContext.Equals("Microsoft.Extensions.Hosting.Internal.Host", StringComparison.Ordinal)
            || sourceContext.StartsWith("Microsoft.Extensions.Hosting.Internal.HostedServiceExecutor", StringComparison.Ordinal)
            || sourceContext.StartsWith("Quartz.", StringComparison.Ordinal))
        {
            return true;
        }

        if (!sourceContext.Contains("Microsoft.EntityFrameworkCore.Database.Command", StringComparison.Ordinal))
            return false;

        var renderedMessage = logEvent.RenderMessage(CultureInfo.InvariantCulture);
        return IsFilteredSqlQuery(renderedMessage);
    }

    private static bool TryGetSourceContext(LogEvent logEvent, out string sourceContext)
    {
        sourceContext = string.Empty;

        if (!logEvent.Properties.TryGetValue("SourceContext", out var value)
            || value is not ScalarValue { Value: string context })
        {
            return false;
        }

        sourceContext = context;
        return true;
    }

    private static bool IsMassTransitOutboxQuery(string? commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            return false;

        return commandText.Contains("outbox_message", StringComparison.OrdinalIgnoreCase)
               || commandText.Contains("outbox_state", StringComparison.OrdinalIgnoreCase)
               || commandText.Contains("inbox_state", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMassTransitOutboxQuery(object command)
        => command is DbCommand dbCommand && IsMassTransitOutboxQuery(dbCommand.CommandText);

    private static bool IsFilteredSqlQuery(object command)
        => command is DbCommand dbCommand && IsFilteredSqlQuery(dbCommand.CommandText);

    private static bool IsFilteredSqlQuery(string? commandText)
        => IsMassTransitOutboxQuery(commandText) || IsHangfireInfrastructureQuery(commandText);

    private static bool IsHangfireInfrastructureQuery(string? commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            return false;

        return commandText.Contains("[HangFire].", StringComparison.OrdinalIgnoreCase)
               || commandText.Contains("[Hangfire].", StringComparison.OrdinalIgnoreCase)
               || commandText.Contains("HangFire.", StringComparison.OrdinalIgnoreCase)
               || commandText.Contains("Hangfire.", StringComparison.OrdinalIgnoreCase)
               || commandText.Contains("sp_getapplock", StringComparison.OrdinalIgnoreCase)
               || commandText.Contains("sp_releaseapplock", StringComparison.OrdinalIgnoreCase)
               || commandText.Trim().Equals("SELECT SYSUTCDATETIME()", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ActivityLogEventEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var activity = Activity.Current;
            if (activity is null)
                return;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));

            var requestId = GetActivityValue(activity, "request.id");
            if (!string.IsNullOrWhiteSpace(requestId))
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", requestId));
        }

        private static string? GetActivityValue(Activity activity, string key)
        {
            foreach (var tag in activity.Tags)
            {
                if (string.Equals(tag.Key, key, StringComparison.Ordinal))
                    return tag.Value;
            }

            foreach (var item in activity.Baggage)
            {
                if (string.Equals(item.Key, key, StringComparison.Ordinal))
                    return item.Value;
            }

            return null;
        }
    }
}

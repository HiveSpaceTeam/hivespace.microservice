using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.Core.Functions.Queueing;

public enum FunctionQueueMode
{
    RabbitMQ = 1,
    AzureServiceBus = 2
}

public sealed class FunctionQueueBackendOptions
{
    public string? ConnectionString { get; set; }
    public string? ConnectionStringName { get; set; }
}

public sealed class FunctionQueueTriggerOptions
{
    public string? Disabled { get; set; }
}

public sealed class FunctionQueueModeValidationResult
{
    private FunctionQueueModeValidationResult(FunctionQueueMode? mode, IReadOnlyList<Error> errors)
    {
        Mode = mode;
        Errors = errors;
    }

    public FunctionQueueMode? Mode { get; }
    public IReadOnlyList<Error> Errors { get; }
    public bool Succeeded => Errors.Count == 0;

    public static FunctionQueueModeValidationResult Success(FunctionQueueMode mode)
        => new(mode, []);

    public static FunctionQueueModeValidationResult Failure(params Error[] errors)
        => new(null, errors);
}

public sealed class FunctionQueueModeOptions
{
    public const string SectionName = "FunctionQueueMode";
    public const string SettingName = "HIVESPACE_FUNCTION_QUEUE_MODE";
    public const string RabbitMqConnectionStringName = "RabbitMq";
    public const string AzureServiceBusConnectionStringName = "AzureServiceBus";
    public const string CatalogImportRabbitMqFunctionDisabledSetting = "AzureWebJobs.CatalogImportRabbitMqFunction.Disabled";
    public const string CatalogImportServiceBusFunctionDisabledSetting = "AzureWebJobs.CatalogImportServiceBusFunction.Disabled";

    public string? Mode { get; set; }
    public FunctionQueueBackendOptions RabbitMQ { get; set; } = new();
    public FunctionQueueBackendOptions AzureServiceBus { get; set; } = new();
    public FunctionQueueTriggerOptions CatalogImportRabbitMqFunction { get; set; } = new();
    public FunctionQueueTriggerOptions CatalogImportServiceBusFunction { get; set; } = new();

    public static FunctionQueueModeOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var rabbitMqSection = section.GetSection(nameof(RabbitMQ));
        var azureServiceBusSection = section.GetSection(nameof(AzureServiceBus));
        var options = new FunctionQueueModeOptions();

        options.Mode = FirstNonEmpty(
            configuration[SettingName],
            section[nameof(Mode)]);

        options.RabbitMQ.ConnectionStringName = FirstNonEmpty(
            rabbitMqSection[nameof(FunctionQueueBackendOptions.ConnectionStringName)],
            RabbitMqConnectionStringName);
        options.RabbitMQ.ConnectionString = FirstNonEmpty(
            rabbitMqSection[nameof(FunctionQueueBackendOptions.ConnectionString)],
            configuration.GetConnectionString(RabbitMqConnectionStringName),
            configuration.GetConnectionString("rabbitmq"),
            configuration.GetConnectionString("RabbitMQ"));

        options.AzureServiceBus.ConnectionStringName = FirstNonEmpty(
            azureServiceBusSection[nameof(FunctionQueueBackendOptions.ConnectionStringName)],
            AzureServiceBusConnectionStringName);
        options.AzureServiceBus.ConnectionString = FirstNonEmpty(
            azureServiceBusSection[nameof(FunctionQueueBackendOptions.ConnectionString)],
            configuration.GetConnectionString(AzureServiceBusConnectionStringName),
            configuration.GetConnectionString("azureservicebus"));
        options.CatalogImportRabbitMqFunction.Disabled = configuration[CatalogImportRabbitMqFunctionDisabledSetting];
        options.CatalogImportServiceBusFunction.Disabled = configuration[CatalogImportServiceBusFunctionDisabledSetting];

        return options;
    }

    public FunctionQueueModeValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(Mode))
        {
            return FunctionQueueModeValidationResult.Failure(
                new Error(CommonErrorCode.ConfigurationMissing, SettingName));
        }

        if (!Enum.TryParse<FunctionQueueMode>(Mode, ignoreCase: true, out var mode))
        {
            return FunctionQueueModeValidationResult.Failure(
                new Error(CommonErrorCode.InvalidArgument, SettingName));
        }

        var selectedBackend = mode switch
        {
            FunctionQueueMode.RabbitMQ => RabbitMQ,
            FunctionQueueMode.AzureServiceBus => AzureServiceBus,
            _ => null
        };

        if (selectedBackend is null || string.IsNullOrWhiteSpace(selectedBackend.ConnectionString))
        {
            return FunctionQueueModeValidationResult.Failure(
                new Error(CommonErrorCode.ConfigurationMissing, GetConnectionStringSource(mode)));
        }

        if (HasCatalogImportTriggerConfiguration())
        {
            var triggerValidationError = ValidateCatalogImportTriggers(mode);
            if (triggerValidationError is not null)
                return FunctionQueueModeValidationResult.Failure(triggerValidationError);
        }

        return FunctionQueueModeValidationResult.Success(mode);
    }

    public FunctionQueueMode GetRequiredMode()
    {
        var result = Validate();
        return result.Mode ?? throw new ConfigurationException(result.Errors);
    }

    private static string GetConnectionStringSource(FunctionQueueMode mode)
        => mode switch
        {
            FunctionQueueMode.RabbitMQ => $"ConnectionStrings:{RabbitMqConnectionStringName}",
            FunctionQueueMode.AzureServiceBus => $"ConnectionStrings:{AzureServiceBusConnectionStringName}",
            _ => "ConnectionStrings"
        };

    private Error? ValidateCatalogImportTriggers(FunctionQueueMode mode)
    {
        var rabbitMqDisabled = ParseDisabled(CatalogImportRabbitMqFunction.Disabled);
        var serviceBusDisabled = ParseDisabled(CatalogImportServiceBusFunction.Disabled);

        return mode switch
        {
            FunctionQueueMode.RabbitMQ when rabbitMqDisabled == true
                => new Error(CommonErrorCode.InvalidArgument, CatalogImportRabbitMqFunctionDisabledSetting),
            FunctionQueueMode.RabbitMQ when serviceBusDisabled != true
                => new Error(CommonErrorCode.InvalidArgument, CatalogImportServiceBusFunctionDisabledSetting),
            FunctionQueueMode.AzureServiceBus when serviceBusDisabled == true
                => new Error(CommonErrorCode.InvalidArgument, CatalogImportServiceBusFunctionDisabledSetting),
            FunctionQueueMode.AzureServiceBus when rabbitMqDisabled != true
                => new Error(CommonErrorCode.InvalidArgument, CatalogImportRabbitMqFunctionDisabledSetting),
            _ => null
        };
    }

    private bool HasCatalogImportTriggerConfiguration()
        => !string.IsNullOrWhiteSpace(CatalogImportRabbitMqFunction.Disabled)
           || !string.IsNullOrWhiteSpace(CatalogImportServiceBusFunction.Disabled);

    private static bool? ParseDisabled(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return bool.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}

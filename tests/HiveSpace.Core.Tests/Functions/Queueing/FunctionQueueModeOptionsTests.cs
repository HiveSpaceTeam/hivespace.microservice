using FluentAssertions;
using HiveSpace.Core.Functions.Queueing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HiveSpace.Core.Tests.Functions.Queueing;

public class FunctionQueueModeOptionsTests
{
    [Fact]
    public void Validate_WithRabbitMqModeAndRabbitMqSettings_Succeeds()
    {
        var options = new FunctionQueueModeOptions
        {
            Mode = "RabbitMQ",
            RabbitMQ = new FunctionQueueBackendOptions { ConnectionString = "amqp://localhost" }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeTrue();
        result.Mode.Should().Be(FunctionQueueMode.RabbitMQ);
    }

    [Fact]
    public void Validate_WithAzureServiceBusModeAndAzureServiceBusSettings_Succeeds()
    {
        var options = new FunctionQueueModeOptions
        {
            Mode = "AzureServiceBus",
            AzureServiceBus = new FunctionQueueBackendOptions { ConnectionString = "Endpoint=sb://local/" }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeTrue();
        result.Mode.Should().Be(FunctionQueueMode.AzureServiceBus);
    }

    [Fact]
    public void Validate_WithMissingMode_Fails()
    {
        var options = new FunctionQueueModeOptions
        {
            RabbitMQ = new FunctionQueueBackendOptions { ConnectionString = "amqp://localhost" }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.Source == FunctionQueueModeOptions.SettingName);
    }

    [Fact]
    public void Validate_WithUnsupportedMode_Fails()
    {
        var options = new FunctionQueueModeOptions
        {
            Mode = "AzureStorageQueue",
            RabbitMQ = new FunctionQueueBackendOptions { ConnectionString = "amqp://localhost" }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.Source == FunctionQueueModeOptions.SettingName);
    }

    [Fact]
    public void Validate_WithInactiveBackendMissing_Succeeds()
    {
        var options = new FunctionQueueModeOptions
        {
            Mode = "RabbitMQ",
            RabbitMQ = new FunctionQueueBackendOptions { ConnectionString = "amqp://localhost" },
            AzureServiceBus = new FunctionQueueBackendOptions { ConnectionString = null }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeTrue();
        result.Mode.Should().Be(FunctionQueueMode.RabbitMQ);
    }

    [Fact]
    public void Validate_WithRabbitMqModeAndCatalogTriggerFlags_Succeeds()
    {
        var options = new FunctionQueueModeOptions
        {
            Mode = "RabbitMQ",
            RabbitMQ = new FunctionQueueBackendOptions { ConnectionString = "amqp://localhost" },
            CatalogImportRabbitMqFunction = new FunctionQueueTriggerOptions { Disabled = "false" },
            CatalogImportServiceBusFunction = new FunctionQueueTriggerOptions { Disabled = "true" }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeTrue();
        result.Mode.Should().Be(FunctionQueueMode.RabbitMQ);
    }

    [Fact]
    public void Validate_WithAzureServiceBusModeAndCatalogTriggerFlags_Succeeds()
    {
        var options = new FunctionQueueModeOptions
        {
            Mode = "AzureServiceBus",
            AzureServiceBus = new FunctionQueueBackendOptions { ConnectionString = "Endpoint=sb://local/" },
            CatalogImportRabbitMqFunction = new FunctionQueueTriggerOptions { Disabled = "true" },
            CatalogImportServiceBusFunction = new FunctionQueueTriggerOptions { Disabled = "false" }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeTrue();
        result.Mode.Should().Be(FunctionQueueMode.AzureServiceBus);
    }

    [Fact]
    public void Validate_WithRabbitMqModeAndServiceBusTriggerEnabled_Fails()
    {
        var options = new FunctionQueueModeOptions
        {
            Mode = "RabbitMQ",
            RabbitMQ = new FunctionQueueBackendOptions { ConnectionString = "amqp://localhost" },
            CatalogImportRabbitMqFunction = new FunctionQueueTriggerOptions { Disabled = "false" },
            CatalogImportServiceBusFunction = new FunctionQueueTriggerOptions { Disabled = "false" }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.Source == FunctionQueueModeOptions.CatalogImportServiceBusFunctionDisabledSetting);
    }

    [Fact]
    public void Validate_WithAzureServiceBusModeAndRabbitMqTriggerEnabled_Fails()
    {
        var options = new FunctionQueueModeOptions
        {
            Mode = "AzureServiceBus",
            AzureServiceBus = new FunctionQueueBackendOptions { ConnectionString = "Endpoint=sb://local/" },
            CatalogImportRabbitMqFunction = new FunctionQueueTriggerOptions { Disabled = "false" },
            CatalogImportServiceBusFunction = new FunctionQueueTriggerOptions { Disabled = "false" }
        };

        var result = options.Validate();

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.Source == FunctionQueueModeOptions.CatalogImportRabbitMqFunctionDisabledSetting);
    }

    [Fact]
    public void FromConfiguration_WithGlobalModeAndCatalogStyleConnectionStrings_BindsOptions()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [FunctionQueueModeOptions.SettingName] = "AzureServiceBus",
            ["ConnectionStrings:AzureServiceBus"] = "Endpoint=sb://local/",
            [FunctionQueueModeOptions.CatalogImportRabbitMqFunctionDisabledSetting] = "true",
            [FunctionQueueModeOptions.CatalogImportServiceBusFunctionDisabledSetting] = "false"
        });

        var options = FunctionQueueModeOptions.FromConfiguration(configuration);

        options.Mode.Should().Be("AzureServiceBus");
        options.AzureServiceBus.ConnectionString.Should().Be("Endpoint=sb://local/");
        options.CatalogImportRabbitMqFunction.Disabled.Should().Be("true");
        options.CatalogImportServiceBusFunction.Disabled.Should().Be("false");
    }

    [Fact]
    public void FromConfiguration_WithLowercaseConnectionStrings_BindsOptions()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [FunctionQueueModeOptions.SettingName] = "RabbitMQ",
            ["ConnectionStrings:rabbitmq"] = "amqp://legacy"
        });

        var options = FunctionQueueModeOptions.FromConfiguration(configuration);

        options.Mode.Should().Be("RabbitMQ");
        options.RabbitMQ.ConnectionString.Should().Be("amqp://legacy");
    }

    [Fact]
    public void AddFunctionQueueMode_WithValidConfiguration_RegistersSelectedMode()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [FunctionQueueModeOptions.SettingName] = "RabbitMQ",
            ["ConnectionStrings:RabbitMq"] = "amqp://localhost"
        });
        var services = new ServiceCollection();

        services.AddFunctionQueueMode(configuration);
        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<FunctionQueueMode>().Should().Be(FunctionQueueMode.RabbitMQ);
        provider.GetRequiredService<FunctionQueueModeOptions>()
            .RabbitMQ.ConnectionString.Should().Be("amqp://localhost");
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}

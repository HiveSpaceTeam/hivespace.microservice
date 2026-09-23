using FluentAssertions;
using HiveSpace.Core.Functions.Queueing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HiveSpace.MediaService.Tests.Application.Queueing;

public class MediaFunctionQueueModeTests
{
    [Fact]
    public void Validate_WithRabbitMqModeAndOnlyRabbitMqSettings_Succeeds()
    {
        var options = FunctionQueueModeOptions.FromConfiguration(BuildConfiguration(new Dictionary<string, string?>
        {
            [FunctionQueueModeOptions.SettingName] = "RabbitMQ",
            ["ConnectionStrings:rabbitmq"] = "amqp://guest:guest@localhost:5672/"
        }));

        var result = options.Validate();

        result.Succeeded.Should().BeTrue();
        result.Mode.Should().Be(FunctionQueueMode.RabbitMQ);
    }

    [Fact]
    public void Validate_WithAzureServiceBusModeAndOnlyAzureServiceBusSettings_Succeeds()
    {
        var options = FunctionQueueModeOptions.FromConfiguration(BuildConfiguration(new Dictionary<string, string?>
        {
            [FunctionQueueModeOptions.SettingName] = "AzureServiceBus",
            ["ConnectionStrings:azureservicebus"] = "Endpoint=sb://local/"
        }));

        var result = options.Validate();

        result.Succeeded.Should().BeTrue();
        result.Mode.Should().Be(FunctionQueueMode.AzureServiceBus);
    }

    [Fact]
    public void Validate_WithMissingMode_Fails()
    {
        var options = FunctionQueueModeOptions.FromConfiguration(BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:rabbitmq"] = "amqp://guest:guest@localhost:5672/"
        }));

        var result = options.Validate();

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.Source == FunctionQueueModeOptions.SettingName);
    }

    [Fact]
    public void Validate_WithUnsupportedMode_Fails()
    {
        var options = FunctionQueueModeOptions.FromConfiguration(BuildConfiguration(new Dictionary<string, string?>
        {
            [FunctionQueueModeOptions.SettingName] = "AzureStorageQueue",
            ["ConnectionStrings:rabbitmq"] = "amqp://guest:guest@localhost:5672/"
        }));

        var result = options.Validate();

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error =>
            error.Source == FunctionQueueModeOptions.SettingName);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}

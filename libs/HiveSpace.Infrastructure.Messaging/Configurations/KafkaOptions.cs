namespace HiveSpace.Infrastructure.Messaging.Configurations;

/// <summary>
/// Kafka specific options for MassTransit riders.
/// </summary>
public class KafkaOptions
{
    public const string SectionName = "Messaging:Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";

    public string ClientId { get; set; } = "hivespace";

    public string ConsumerGroup { get; set; } = "hivespace-consumers";

    public string? SaslUsername { get; set; }

    public string? SaslPassword { get; set; }

    public string SecurityProtocol { get; set; } = "PLAINTEXT";
}



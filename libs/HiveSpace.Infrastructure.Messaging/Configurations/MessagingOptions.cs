namespace HiveSpace.Infrastructure.Messaging.Configurations;

/// <summary>
/// Represents the root configuration section used by messaging extensions.
/// </summary>
public class MessagingOptions
{
    public const string SectionName = "Messaging";

    public bool EnableRabbitMq { get; set; } = true;

    public bool EnableKafka { get; set; } = false;

    public RabbitMqOptions RabbitMq { get; set; } = new();

    public KafkaOptions Kafka { get; set; } = new();
}



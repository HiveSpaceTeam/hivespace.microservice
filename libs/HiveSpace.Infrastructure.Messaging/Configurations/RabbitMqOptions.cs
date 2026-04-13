namespace HiveSpace.Infrastructure.Messaging.Configurations;

/// <summary>
/// RabbitMQ specific options that can be bound from configuration.
/// </summary>
public class RabbitMqOptions
{
    public const string SectionName = "Messaging:RabbitMq";

    public string Host { get; set; } = "localhost";

    public ushort Port { get; set; } = 5672;

    public string VirtualHost { get; set; } = "/";

    public string Username { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string ClientName { get; set; } = "hivespace";

    public ushort PrefetchCount { get; set; } = 16;

    public bool UseSsl { get; set; } = false;

    public ushort HeartBeat { get; set; } = 60;

    public ushort RetryCount { get; set; } = 3;

    public int DuplicateDetectionWindowMinutes { get; set; } = 5;

    public int OutboxQueryDelaySeconds { get; set; } = 1;

    public int OutboxQueryMessageLimit { get; set; } = 100;

    public int OutboxQueryTimeoutSeconds { get; set; } = 60;

    public int OutboxMessageDeliveryLimit { get; set; } = 50;
}


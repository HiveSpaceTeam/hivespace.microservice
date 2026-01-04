namespace HiveSpace.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Base contract for command messages that travel across microservices.
/// </summary>
public interface ICommand
{
    Guid CommandId { get; }
    
    DateTimeOffset CreatedAt { get; }
}



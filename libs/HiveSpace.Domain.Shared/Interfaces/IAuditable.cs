namespace HiveSpace.Domain.Shared.Interfaces;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
}
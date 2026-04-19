namespace HiveSpace.Infrastructure.Messaging.Shared.Events.Users;

public record UserCreatedEvent
{
    public Guid    UserId      { get; init; }
    public string  Email       { get; init; } = default!;
    public string  FullName    { get; init; } = default!;
    public string? PhoneNumber { get; init; }
    public string  Locale      { get; init; } = "vi";
    public string? UserName    { get; init; }
    public string? AvatarUrl   { get; init; }
}

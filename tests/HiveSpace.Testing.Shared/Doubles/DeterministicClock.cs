namespace HiveSpace.Testing.Shared.Doubles;

public sealed class DeterministicClock(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}

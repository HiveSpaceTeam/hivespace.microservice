using HiveSpace.Domain.Shared.IdGeneration;

namespace HiveSpace.Testing.Shared.Doubles;

public sealed class SequentialLongGenerator : IIdGenerator<long>
{
    private long _sequence;

    public long NewId()
    {
        return Interlocked.Increment(ref _sequence);
    }
}

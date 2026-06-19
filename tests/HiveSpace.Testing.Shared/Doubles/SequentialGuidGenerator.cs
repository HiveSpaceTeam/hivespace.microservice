using HiveSpace.Domain.Shared.IdGeneration;

namespace HiveSpace.Testing.Shared.Doubles;

public sealed class SequentialGuidGenerator : IIdGenerator<Guid>
{
    private long _sequence;

    public Guid NewId()
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(Interlocked.Increment(ref _sequence)).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}

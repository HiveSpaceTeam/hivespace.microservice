namespace HiveSpace.Domain.Shared.IdGeneration;

/// <summary>
/// Static gateway for generating IDs inside domain entities without constructor injection.
/// Initialized once at startup by the infrastructure layer via <see cref="Initialize"/>.
/// </summary>
public static class IdGenerator
{
    private static IIdGenerator<Guid>? _guidGen;
    private static IIdGenerator<long>? _longGen;

    /// <summary>Called by the infrastructure layer (e.g. AddIdGenerators) during startup.</summary>
    public static void Initialize(IIdGenerator<Guid> guidGen, IIdGenerator<long> longGen)
    {
        _guidGen = guidGen;
        _longGen = longGen;
    }

    /// <summary>Generates a new ID of type <typeparamref name="T"/>.</summary>
    public static T NewId<T>() where T : notnull =>
        typeof(T) switch
        {
            var t when t == typeof(Guid)  => (T)(object)Resolve(_guidGen).NewId(),
            var t when t == typeof(long)  => (T)(object)Resolve(_longGen).NewId(),
            _ => throw new NotSupportedException(
                     $"No ID generator registered for type '{typeof(T).Name}'. Supported: Guid, long.")
        };

    private static TGen Resolve<TGen>(TGen? gen) where TGen : class =>
        gen ?? throw new InvalidOperationException(
            "IdGenerator has not been initialized. Ensure AddIdGenerators() is called at startup.");
}

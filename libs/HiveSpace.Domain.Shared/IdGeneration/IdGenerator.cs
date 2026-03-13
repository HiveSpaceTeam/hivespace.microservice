using HiveSpace.Domain.Shared.Errors;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.Domain.Shared.IdGeneration;

/// <summary>
/// Static gateway for generating IDs inside domain entities without constructor injection.
/// Initialized exactly once at startup by the infrastructure layer via <see cref="Initialize"/>.
/// </summary>
public static class IdGenerator
{
    // Holds both generators in a single immutable container so callers never
    // observe a state where one generator is set but the other is not.
    private sealed record Generators(IIdGenerator<Guid> GuidGen, IIdGenerator<long> LongGen);

    private static Generators? _generators;

    /// <summary>
    /// Called by the infrastructure layer (e.g. AddIdGenerators) during startup.
    /// May be called only once; a second call throws <see cref="DomainException"/>.
    /// </summary>
    public static void Initialize(IIdGenerator<Guid> guidGen, IIdGenerator<long> longGen)
    {
        var next = new Generators(guidGen, longGen);
        if (Interlocked.CompareExchange(ref _generators, next, null) != null)
            throw new DomainException(500, DomainErrorCode.InvalidExpression, nameof(IdGenerator));
    }

    /// <summary>Generates a new ID of type <typeparamref name="T"/>.</summary>
    public static T NewId<T>() where T : notnull
    {
        var gens = Volatile.Read(ref _generators)
            ?? throw new DomainException(500, DomainErrorCode.ParameterRequired, nameof(Initialize));

        return typeof(T) switch
        {
            var t when t == typeof(Guid) => (T)(object)gens.GuidGen.NewId(),
            var t when t == typeof(long) => (T)(object)gens.LongGen.NewId(),
            _ => throw new InvalidFieldException(DomainErrorCode.InvalidExpression, nameof(NewId))
        };
    }
}

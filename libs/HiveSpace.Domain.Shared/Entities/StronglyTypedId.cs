using HiveSpace.Domain.Shared.Errors;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.Domain.Shared.Entities;

/// <summary>
/// Base class for strongly-typed identifiers.
/// Provides type safety to prevent mixing different entity IDs.
/// </summary>
public abstract class StronglyTypedId<T> : ValueObject where T : notnull
{
    public T Value { get; }

    protected StronglyTypedId(T value)
    {
        if (value == null)
            throw new InvalidFieldException(DomainErrorCode.ArgumentNull, nameof(value));

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public static implicit operator T(StronglyTypedId<T> id) => id.Value;

}

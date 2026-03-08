using System;
using System.Linq.Expressions;

namespace HiveSpace.Domain.Shared.Specifications;

/// <summary>
/// Base implementation of specification pattern
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    /// <summary>
    /// Checks if entity satisfies the specification
    /// </summary>
    public virtual bool IsSatisfiedBy(T obj)
    {
        return ToExpression().Compile()(obj);
    }

    /// <summary>
    /// Returns the expression representing the specification
    /// </summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Combines this specification with another using AND logic
    /// </summary>
    public virtual Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    /// <summary>
    /// Combines this specification with another using OR logic
    /// </summary>
    public virtual Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    /// <summary>
    /// Negates the specification
    /// </summary>
    public virtual Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }

    /// <summary>
    /// Implicitly converts specification to expression
    /// </summary>
    public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
    {
        return specification.ToExpression();
    }

    /// <summary>
    /// Implicitly converts specification to Func
    /// </summary>
    public static implicit operator Func<T, bool>(Specification<T> specification)
    {
        return specification.ToExpression().Compile();
    }
}

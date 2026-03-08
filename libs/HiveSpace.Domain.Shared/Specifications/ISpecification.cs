using System;
using System.Linq.Expressions;

namespace HiveSpace.Domain.Shared.Specifications;

/// <summary>
/// Base specification interface following ABP Framework pattern
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Checks if the given object satisfies the specification
    /// </summary>
    bool IsSatisfiedBy(T obj);

    /// <summary>
    /// Gets the expression that represents the specification
    /// </summary>
    Expression<Func<T, bool>> ToExpression();
}

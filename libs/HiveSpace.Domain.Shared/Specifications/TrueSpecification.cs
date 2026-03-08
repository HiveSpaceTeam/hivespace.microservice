using System;
using System.Linq.Expressions;

namespace HiveSpace.Domain.Shared.Specifications;

/// <summary>
/// A specification that is always true
/// </summary>
public class TrueSpecification<T> : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        return x => true;
    }
}

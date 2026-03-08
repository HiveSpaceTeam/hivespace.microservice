using System;
using System.Linq.Expressions;

namespace HiveSpace.Domain.Shared.Specifications;

/// <summary>
/// A specification that is always false
/// </summary>
public class FalseSpecification<T> : Specification<T>
{
    public override Expression<Func<T, bool>> ToExpression()
    {
        return x => false;
    }
}

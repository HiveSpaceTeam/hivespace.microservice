using System;
using System.Linq.Expressions;

namespace HiveSpace.Domain.Shared.Specifications;

/// <summary>
/// Negates a specification
/// </summary>
public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = expression.Parameters[0];

        return Expression.Lambda<Func<T, bool>>(
            Expression.Not(expression.Body),
            parameter
        );
    }
}

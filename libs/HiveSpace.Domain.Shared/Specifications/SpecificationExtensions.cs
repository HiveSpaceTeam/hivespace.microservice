using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HiveSpace.Domain.Shared.Specifications;

public static class SpecificationExtensions
{
    /// <summary>
    /// Combines specifications with AND
    /// </summary>
    public static Specification<T> And<T>(
        this Specification<T> left,
        Expression<Func<T, bool>> right)
    {
        return left.And(new ExpressionSpecification<T>(right));
    }

    /// <summary>
    /// Combines specifications with OR
    /// </summary>
    public static Specification<T> Or<T>(
        this Specification<T> left,
        Expression<Func<T, bool>> right)
    {
        return left.Or(new ExpressionSpecification<T>(right));
    }

    /// <summary>
    /// Adds AND condition only if predicate is true
    /// </summary>
    public static Specification<T> AndIf<T>(
        this Specification<T> specification,
        bool condition,
        Specification<T> spec)
    {
        return condition ? specification.And(spec) : specification;
    }

    /// <summary>
    /// Adds AND condition only if predicate is true
    /// </summary>
    public static Specification<T> AndIf<T>(
        this Specification<T> specification,
        bool condition,
        Expression<Func<T, bool>> expression)
    {
        return condition 
            ? specification.And(new ExpressionSpecification<T>(expression)) 
            : specification;
    }

    /// <summary>
    /// Adds OR condition only if predicate is true
    /// </summary>
    public static Specification<T> OrIf<T>(
        this Specification<T> specification,
        bool condition,
        Specification<T> spec)
    {
        return condition ? specification.Or(spec) : specification;
    }

    /// <summary>
    /// Applies specification to IQueryable
    /// </summary>
    public static IQueryable<T> Where<T>(
        this IQueryable<T> query,
        Specification<T> specification)
    {
        return query.Where(specification.ToExpression());
    }

    /// <summary>
    /// Applies specification to IEnumerable
    /// </summary>
    public static IEnumerable<T> Where<T>(
        this IEnumerable<T> collection,
        Specification<T> specification)
    {
        return collection.Where(specification.ToExpression().Compile());
    }

    /// <summary>
    /// Checks if any item satisfies the specification
    /// </summary>
    public static bool Any<T>(
        this IEnumerable<T> collection,
        Specification<T> specification)
    {
        return collection.Any(specification.ToExpression().Compile());
    }

    /// <summary>
    /// Checks if all items satisfy the specification
    /// </summary>
    public static bool All<T>(
        this IEnumerable<T> collection,
        Specification<T> specification)
    {
        return collection.All(specification.ToExpression().Compile());
    }

    /// <summary>
    /// Counts items that satisfy the specification
    /// </summary>
    public static int Count<T>(
        this IEnumerable<T> collection,
        Specification<T> specification)
    {
        return collection.Count(specification.ToExpression().Compile());
    }

    /// <summary>
    /// Gets first item that satisfies the specification
    /// </summary>
    public static T? FirstOrDefault<T>(
        this IEnumerable<T> collection,
        Specification<T> specification)
    {
        return collection.FirstOrDefault(specification.ToExpression().Compile());
    }
}

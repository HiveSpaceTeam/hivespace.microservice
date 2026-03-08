using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.Aggregates.Coupons;

/// <summary>
/// CouponRule entity - custom validation rules for coupons
/// </summary>
public class CouponRule : Entity<Guid>
{
    public string RuleName { get; private set; } = null!;
    public string RuleExpression { get; private set; } = null!;
    public string ErrorMessage { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private CouponRule() { }

    private CouponRule(string ruleName, string ruleExpression, string errorMessage)
    {
        Id = Guid.NewGuid();
        RuleName = ruleName;
        RuleExpression = ruleExpression;
        ErrorMessage = errorMessage;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static CouponRule Create(string ruleName, string ruleExpression, string errorMessage)
    {
        return new CouponRule(ruleName, ruleExpression, errorMessage);
    }
}

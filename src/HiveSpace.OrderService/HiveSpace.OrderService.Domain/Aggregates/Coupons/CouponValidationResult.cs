namespace HiveSpace.OrderService.Domain.Aggregates.Coupons;

/// <summary>
/// Coupon validation result
/// </summary>
public class CouponValidationResult(bool isValid, IEnumerable<string>? errors = null)
{
    public bool IsValid { get; } = isValid;
    public IReadOnlyList<string> Errors { get; } = errors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();

    public static CouponValidationResult Valid() => new(true);
    public static CouponValidationResult Invalid(params string[] errors) => new(false, errors);
}

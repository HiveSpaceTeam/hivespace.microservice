using System.Collections.Generic;
using System.Linq;
using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.OrderService.Domain.Aggregates.Coupons;

public record CouponValidationError(DomainErrorCode ErrorCode, string Source);

/// <summary>
/// Coupon validation result
/// </summary>
public class CouponValidationResult(bool isValid, IEnumerable<CouponValidationError>? errors = null)
{
    public bool IsValid { get; } = isValid;
    public IReadOnlyList<CouponValidationError> Errors { get; } = errors?.ToList().AsReadOnly() ?? new List<CouponValidationError>().AsReadOnly();

    public static CouponValidationResult Valid() => new(true);
    public static CouponValidationResult Invalid(params CouponValidationError[] errors) => new(false, errors);
}

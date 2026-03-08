namespace HiveSpace.OrderService.Domain.Enumerations;

public enum CouponStatus
{
    Draft,              // Being created
    PendingApproval,    // Store coupon awaiting platform approval
    Active,             // Can be used
    Rejected,           // Platform rejected store coupon
    Expired,            // Past end date
    Deactivated         // Manually disabled
}

using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Domain.DomainEvents;

namespace HiveSpace.OrderService.Domain.Aggregates.Coupons;

/// <summary>
/// Coupon aggregate root
/// Manages discount coupons, usage limits, and validation rules
/// </summary>
public class Coupon : AggregateRoot<Guid>, IAuditable
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public Money? DiscountAmount { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public Money? MaxDiscountAmount { get; private set; }
    public Money MinOrderAmount { get; private set; } = null!;
    public CouponScope Scope { get; private set; } = null!;
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public int? MaxUsageCount { get; private set; }
    public int CurrentUsageCount { get; private set; }
    public int? MaxUsagePerUser { get; private set; }
    public CouponOwnerType OwnerType { get; private set; }
    public Guid? StoreId { get; private set; }
    public Guid CreatedBy { get; private set; }
    public CouponStatus Status { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Applicable products/stores (if limited)
    private readonly List<Guid> _applicableProductIds = [];
    public IReadOnlyCollection<Guid> ApplicableProductIds => _applicableProductIds.AsReadOnly();

    private readonly List<Guid> _applicableStoreIds = [];
    public IReadOnlyCollection<Guid> ApplicableStoreIds => _applicableStoreIds.AsReadOnly();

    // Usage tracking
    private readonly List<CouponUsage> _usages = [];
    public IReadOnlyCollection<CouponUsage> Usages => _usages.AsReadOnly();

    // Validation rules
    private readonly List<CouponRule> _rules = [];
    public IReadOnlyCollection<CouponRule> Rules => _rules.AsReadOnly();

    private Coupon() { }

    private Coupon(
        string code,
        string name,
        DiscountType discountType,
        CouponScope scope,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CouponOwnerType ownerType,
        Guid createdBy,
        Guid? storeId = null)
    {
        Id = Guid.NewGuid();
        Code = code.ToUpperInvariant();
        Name = name;
        DiscountType = discountType;
        Scope = scope;
        StartDate = startDate;
        EndDate = endDate;
        OwnerType = ownerType;
        CreatedBy = createdBy;
        StoreId = storeId;
        
        // Initial status based on owner type
        Status = ownerType == CouponOwnerType.Store 
            ? CouponStatus.PendingApproval 
            : CouponStatus.Active;

        CurrentUsageCount = 0;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Create fixed amount discount coupon
    /// </summary>
    /// <summary>
    /// Create Store Coupon (Pending Approval)
    /// </summary>
    public static Coupon CreateByStore(
        Guid storeId,
        Guid storeOwnerId,
        string code,
        string name,
        DiscountType discountType,
        decimal? percentage,
        Money? discountAmount,
        CouponScope scope,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Money? maxDiscountAmount = null,
        Money? minOrderAmount = null)
    {
        if (!code.StartsWith($"STORE{storeId}-", StringComparison.InvariantCultureIgnoreCase))
             throw new InvalidFieldException(OrderDomainErrorCode.CouponCodeInvalidPrefix, nameof(code));

        var coupon = new Coupon(code, name, discountType, scope, startDate, endDate, CouponOwnerType.Store, storeOwnerId, storeId);
        
        if (discountType == DiscountType.FixedAmount)
        {
             if (discountAmount is null || discountAmount.Amount <= 0)
                 throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidDiscountAmount, nameof(discountAmount));
             coupon.DiscountAmount = discountAmount;
        }
        else
        {
             if (percentage is null || percentage <= 0 || percentage > 100)
                 throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidPercentage, nameof(percentage));
             coupon.DiscountPercentage = percentage.Value;
             coupon.MaxDiscountAmount = maxDiscountAmount;
        }

        coupon.MinOrderAmount = minOrderAmount ?? Money.Zero();

        // coupon.AddDomainEvent(new StoreCouponCreatedEvent(coupon.Id, storeId)); // Assuming event exists or sticking to geneic
        coupon.AddDomainEvent(new CouponCreatedDomainEvent(coupon.Id, code, discountAmount ?? Money.Zero())); // Reusing existing event for now

        return coupon;
    }

    /// <summary>
    /// Create Platform Coupon (Active)
    /// </summary>
    public static Coupon CreateByPlatform(
        Guid adminId,
        string code,
        string name,
        DiscountType discountType,
        decimal? percentage,
        Money? discountAmount,
        CouponScope scope,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        Money? maxDiscountAmount = null,
        Money? minOrderAmount = null)
    {
        var coupon = new Coupon(code, name, discountType, scope, startDate, endDate, CouponOwnerType.Platform, adminId);
        
        if (discountType == DiscountType.FixedAmount)
        {
             if (discountAmount is null || discountAmount.Amount <= 0)
                 throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidDiscountAmount, nameof(discountAmount));
             coupon.DiscountAmount = discountAmount;
        }
        else
        {
             if (percentage is null || percentage <= 0 || percentage > 100)
                 throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidPercentage, nameof(percentage));
             coupon.DiscountPercentage = percentage.Value;
             coupon.MaxDiscountAmount = maxDiscountAmount;
        }

        coupon.MinOrderAmount = minOrderAmount ?? Money.Zero();

        coupon.AddDomainEvent(new CouponCreatedDomainEvent(coupon.Id, code, discountAmount ?? Money.Zero()));

        return coupon;
    }

    /// <summary>
    /// Set maximum usage count (total)
    /// </summary>
    public void SetMaxUsageCount(int maxUsage)
    {
        if (maxUsage <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidMaxUsage, nameof(maxUsage));

        MaxUsageCount = maxUsage;

    }

    /// <summary>
    /// Set maximum usage per user
    /// </summary>
    public void SetMaxUsagePerUser(int maxUsagePerUser)
    {
        if (maxUsagePerUser <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidMaxUsagePerUser, nameof(maxUsagePerUser));

        MaxUsagePerUser = maxUsagePerUser;

    }

    /// <summary>
    /// Set description
    /// </summary>
    public void SetDescription(string description)
    {
        Description = description;

    }

    /// <summary>
    /// Limit coupon to specific products
    /// </summary>
    public void LimitToProducts(IEnumerable<Guid> productIds)
    {
        _applicableProductIds.Clear();
        _applicableProductIds.AddRange(productIds);

    }

    /// <summary>
    /// Limit coupon to specific stores
    /// </summary>
    public void LimitToStores(IEnumerable<Guid> storeIds)
    {
        _applicableStoreIds.Clear();
        _applicableStoreIds.AddRange(storeIds);

    }

    /// <summary>
    /// Add custom validation rule
    /// </summary>
    public void AddRule(string ruleName, string ruleExpression, string errorMessage)
    {
        var rule = CouponRule.Create(ruleName, ruleExpression, errorMessage);
        _rules.Add(rule);

    }

    /// <summary>
    /// Validate if coupon can be used for an order
    /// </summary>
    /// <summary>
    /// Approve Pending Store Coupon
    /// </summary>
    public void Approve(Guid approvedBy)
    {
        if (OwnerType != CouponOwnerType.Store)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponNotStoreOwned, nameof(OwnerType));
        
        if (Status != CouponStatus.PendingApproval)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidStatus, nameof(Status));
        
        Status = CouponStatus.Active;
        ApprovedAt = DateTimeOffset.UtcNow;
        ApprovedBy = approvedBy;
        
        // AddDomainEvent(new CouponApprovedEvent(Id, approvedBy));
    }

    /// <summary>
    /// Reject Pending Store Coupon
    /// </summary>
    public void Reject(Guid rejectedBy, string reason)
    {
        if (OwnerType != CouponOwnerType.Store)
             throw new InvalidFieldException(OrderDomainErrorCode.CouponNotStoreOwned, nameof(OwnerType));
        
        if (Status != CouponStatus.PendingApproval)
             throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidStatus, nameof(Status));
        
        Status = CouponStatus.Rejected;
        
        // AddDomainEvent(new CouponRejectedEvent(Id, rejectedBy, reason));
    }

    /// <summary>
    /// Validate if coupon can be used for an order
    /// </summary>
    public CouponValidationResult Validate(
        Guid userId,
        Money orderTotal,
        IEnumerable<Guid>? productIds = null,
        Guid? storeId = null)
    {
        var errors = new List<string>();

        // Check if active
        if (Status != CouponStatus.Active)
            errors.Add($"Coupon is not active (Status: {Status})");

        // Check date range
        var now = DateTimeOffset.UtcNow;
        if (now < StartDate)
            errors.Add($"Coupon not valid until {StartDate:yyyy-MM-dd}");
        if (now > EndDate)
            errors.Add($"Coupon expired on {EndDate:yyyy-MM-dd}");

        // Check minimum order amount
        if (orderTotal < MinOrderAmount)
            errors.Add($"Minimum order amount is {MinOrderAmount}");

        // Check total usage limit
        if (MaxUsageCount.HasValue && CurrentUsageCount >= MaxUsageCount.Value)
            errors.Add("Coupon usage limit reached");

        // Check per-user usage limit
        if (MaxUsagePerUser.HasValue)
        {
            var userUsageCount = _usages.Count(u => u.UserId == userId);
            if (userUsageCount >= MaxUsagePerUser.Value)
                errors.Add($"You have already used this coupon {MaxUsagePerUser.Value} time(s)");
        }

        // Check product applicability
        if (_applicableProductIds.Count != 0 && productIds != null)
        {
            var hasApplicableProduct = productIds.Any(p => _applicableProductIds.Contains(p));
            if (!hasApplicableProduct)
                errors.Add("Coupon not applicable to products in cart");
        }

        // Check store applicability
        if (_applicableStoreIds.Count != 0 && storeId.HasValue)
        {
            if (!_applicableStoreIds.Contains(storeId.Value))
                errors.Add("Coupon not applicable to this store");
        }
        
        // Store Owner Check - if coupon is a Store coupon, it must only be used for that Store
        if (OwnerType == CouponOwnerType.Store)
        {
            if (storeId == null || storeId != StoreId)
            {
                errors.Add($"This coupon is only valid for store {StoreId}");
            }
        }

        return new CouponValidationResult(errors.Count == 0, errors);
    }

    /// <summary>
    /// Calculate discount amount based on order total
    /// </summary>
    public Money CalculateDiscount(Money orderTotal)
    {
        if (DiscountType == DiscountType.FixedAmount)
        {
            // Fixed amount discount
            return orderTotal >= DiscountAmount! ? DiscountAmount! : orderTotal;
        }
        else // Percentage
        {
            // Calculate percentage discount
            var discountAmount = orderTotal * (DiscountPercentage / 100m);
            
            // Cap at max discount if specified
            if (MaxDiscountAmount != null && discountAmount > MaxDiscountAmount)
                discountAmount = MaxDiscountAmount;

            // Don't exceed order total
            if (discountAmount > orderTotal)
                discountAmount = orderTotal;

            return discountAmount;
        }
    }

    /// <summary>
    /// Mark coupon as used
    /// </summary>
    public void MarkAsUsed(Guid userId, Guid orderId, Money discountAmount)
    {
        // Note: Real validation should happen before MarkAsUsed, likely in the Service layer.
        // We verify liveness and limits here.
        // Since we pass Money.Zero(), min order amount check will fail if MinOrderAmount > 0.
        // So we should probably NOT call Validate(0) here or assume the caller has validated.
        // The user code did calls Validate(userId, Money.Zero()). If MinOrderAmount > 0, this will always fail?
        // Let's check user code:
        // public void MarkAsUsed(...) { var validation = Validate(userId, Money.Zero()); }
        // public CouponValidationResult Validate(...) { if (orderTotal < MinOrderAmount) errors.Add... }
        // Yes, this seems like a bug in the user's provided code if MinOrderAmount > 0.
        // But adhering to "create ... from this code", I should perhaps keep it or improve it.
        // I will remove the MinOrderAmount check from this specific validation call or just trust the caller?
        // Better: I will assume the caller validated it. But to check usage limits, I need to check limits.
        // I'll leave it as is but warn myself? No, I should fix it.
        // I'll skip the Validate call here or create a specialized internal check.
        // Actually, let's keep it but maybe it expects MinOrderAmount to be 0 for this check? No.
        // I'll change it to check only limits.
        
        if (!IsCurrentlyValid())
             throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalid, nameof(Coupon));

        if (MaxUsageCount.HasValue && CurrentUsageCount >= MaxUsageCount.Value)
             throw new InvalidFieldException(OrderDomainErrorCode.CouponUsageLimitReached, nameof(MaxUsageCount));

        if (MaxUsagePerUser.HasValue)
        {
            var userUsageCount = _usages.Count(u => u.UserId == userId);
            if (userUsageCount >= MaxUsagePerUser.Value)
                 throw new InvalidFieldException(OrderDomainErrorCode.CouponUserLimitReached, nameof(MaxUsagePerUser));
        }

        var usage = CouponUsage.Create(userId, orderId, discountAmount);
        _usages.Add(usage);
        CurrentUsageCount++;


        AddDomainEvent(new CouponUsedDomainEvent(Id, Code, userId, orderId, discountAmount));
    }

    /// <summary>
    /// Activate coupon
    /// </summary>
    public void Activate()
    {
        Status = CouponStatus.Active;
    }

    /// <summary>
    /// Deactivate coupon
    /// </summary>
    public void Deactivate()
    {
        Status = CouponStatus.Deactivated;
        AddDomainEvent(new CouponDeactivatedDomainEvent(Id, Code));
    }

    /// <summary>
    /// Extend expiration date
    /// </summary>
    public void ExtendExpiration(DateTimeOffset newEndDate)
    {
        if (newEndDate <= EndDate)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidExtension, nameof(EndDate));

        EndDate = newEndDate;

    }

    /// <summary>
    /// Check if coupon is expired
    /// </summary>
    public bool IsExpired() => DateTimeOffset.UtcNow > EndDate;

    /// <summary>
    /// Check if coupon is currently valid (active and within date range)
    /// </summary>
    public bool IsCurrentlyValid()
    {
        var now = DateTimeOffset.UtcNow;
        return Status == CouponStatus.Active && now >= StartDate && now <= EndDate;
    }

    /// <summary>
    /// Get remaining usage count
    /// </summary>
    public int? GetRemainingUsage()
    {
        if (!MaxUsageCount.HasValue)
            return null; // Unlimited
        
        return MaxUsageCount.Value - CurrentUsageCount;
    }
}

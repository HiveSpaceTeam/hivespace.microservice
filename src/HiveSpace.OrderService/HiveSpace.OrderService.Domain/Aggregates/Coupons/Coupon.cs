using HiveSpace.Domain.Shared.IdGeneration;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
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
    public DiscountType DiscountType { get; private set; }
    public Money? DiscountAmount { get; private set; }
    public decimal? DiscountPercentage { get; private set; }
    public Money? MaxDiscountAmount { get; private set; }
    public Money MinOrderAmount { get; private set; } = null!;
    public CouponScope Scope { get; private set; }
    public DateTimeOffset StartDateTime { get; private set; }
    public DateTimeOffset EndDateTime { get; private set; }
    public DateTimeOffset? EarlySaveDateTime { get; private set; }
    public bool IsHidden { get; private set; }
    public int MaxUsageCount { get; private set; }
    public int CurrentUsageCount { get; private set; }
    public int MaxUsagePerUser { get; private set; }
    public CouponOwnerType OwnerType { get; private set; }
    public string CreatedBy { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Applicable products/stores (if limited)
    private readonly List<long> _applicableProductIds = [];
    public IReadOnlyCollection<long> ApplicableProductIds => _applicableProductIds.AsReadOnly();

    public Guid? StoreId { get; private set; }

    private readonly List<int> _applicableCategoryIds = [];
    public IReadOnlyCollection<int> ApplicableCategoryIds => _applicableCategoryIds.AsReadOnly();

    // Usage tracking
    private readonly List<CouponUsage> _usages = [];
    public IReadOnlyCollection<CouponUsage> Usages => _usages.AsReadOnly();

    // Validation rules
    private readonly List<CouponRule> _rules = [];
    public IReadOnlyCollection<CouponRule> Rules => _rules.AsReadOnly();

    private Coupon() { }

    private Coupon(
        Guid id,
        string code,
        string name,
        DiscountType discountType,
        CouponScope scope,
        DateTimeOffset startDateTime,
        DateTimeOffset endDateTime,
        DateTimeOffset? earlySaveDateTime,
        bool isHidden,
        CouponOwnerType ownerType,
        string createdBy)
    {
        Id = id;
        Code = code.ToUpperInvariant();
        Name = name;
        DiscountType = discountType;
        Scope = scope;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        EarlySaveDateTime = earlySaveDateTime;
        IsHidden = isHidden;
        OwnerType = ownerType;
        CreatedBy = createdBy;
        
        // Initial status is Active by default
        IsActive = true;

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
        DateTimeOffset startDateTime,
        DateTimeOffset endDateTime,
        DateTimeOffset? earlySaveDateTime = null,
        bool isHidden = false,
        Money? maxDiscountAmount = null,
        Money? minOrderAmount = null)
    {
        //    if (!code.StartsWith($"STORE{storeId}-", StringComparison.InvariantCultureIgnoreCase))
        //         throw new InvalidFieldException(OrderDomainErrorCode.CouponCodeInvalidPrefix, nameof(code));

        var coupon = new Coupon(IdGenerator.NewId<Guid>(), code, name, discountType, scope, startDateTime, endDateTime, earlySaveDateTime, isHidden, CouponOwnerType.Store, storeOwnerId.ToString());
        coupon.StoreId = storeId;
        
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

        if (discountType == DiscountType.Percentage && maxDiscountAmount != null)
        {
            if (maxDiscountAmount.Amount < (coupon.MinOrderAmount.Amount * (percentage!.Value / 100m)))
            {
                throw new InvalidFieldException(OrderDomainErrorCode.CouponMaxDiscountTooSmall, nameof(maxDiscountAmount));
            }
        }

        // coupon.AddDomainEvent(new StoreCouponCreatedEvent(coupon.Id, storeId)); // Assuming event exists or sticking to geneic
        coupon.AddDomainEvent(new CouponCreatedDomainEvent(coupon.Id, code, discountAmount ?? Money.Zero())); // Reusing existing event for now

        return coupon;
    }

    /// <summary>
    /// Create Platform Coupon (Active)
    /// </summary>
    public static Coupon CreateByPlatform(
        string adminId,
        string code,
        string name,
        DiscountType discountType,
        decimal? percentage,
        Money? discountAmount,
        CouponScope scope,
        DateTimeOffset startDateTime,
        DateTimeOffset endDateTime,
        DateTimeOffset? earlySaveDateTime = null,
        bool isHidden = false,
        Money? maxDiscountAmount = null,
        Money? minOrderAmount = null)
    {
        var coupon = new Coupon(IdGenerator.NewId<Guid>(), code, name, discountType, scope, startDateTime, endDateTime, earlySaveDateTime, isHidden, CouponOwnerType.Platform, adminId);
        
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

        if (discountType == DiscountType.Percentage && maxDiscountAmount != null)
        {
            if (maxDiscountAmount.Amount < (coupon.MinOrderAmount.Amount * (percentage!.Value / 100m)))
            {
                throw new InvalidFieldException(OrderDomainErrorCode.CouponMaxDiscountTooSmall, nameof(maxDiscountAmount));
            }
        }

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
    public void SetIsHidden(bool isHidden)
    {
        IsHidden = isHidden;

    }

    /// <summary>
    /// Limit coupon to specific products
    /// </summary>
    public void LimitToProducts(IEnumerable<long> productIds)
    {
        _applicableProductIds.Clear();
        _applicableProductIds.AddRange(productIds);

    }



    /// <summary>
    /// Limit coupon to specific categories
    /// </summary>
    public void LimitToCategories(IEnumerable<int> categoryIds)
    {
        _applicableCategoryIds.Clear();
        _applicableCategoryIds.AddRange(categoryIds);

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
    /// Validate if coupon can be used for an order
    /// </summary>
    public CouponValidationResult Validate(
        Guid userId,
        Money orderTotal,
        IEnumerable<long>? productIds = null,
        Guid? storeId = null,
        IEnumerable<int>? categoryIds = null)
    {
        var errors = new List<CouponValidationError>();

        // Check if active
        if (!IsActive)
            errors.Add(new(OrderDomainErrorCode.CouponNotActive, nameof(IsActive)));

        // Check date range
        var now = DateTimeOffset.UtcNow;
        if (now < StartDateTime)
            errors.Add(new(OrderDomainErrorCode.CouponInvalidDates, nameof(StartDateTime)));
        if (now > EndDateTime)
            errors.Add(new(OrderDomainErrorCode.CouponExpired, nameof(EndDateTime)));

        // Check minimum order amount
        if (orderTotal < MinOrderAmount)
            errors.Add(new(OrderDomainErrorCode.CouponMinOrderAmountNotMet, nameof(MinOrderAmount)));

        // Check total usage limit
        if (MaxUsageCount > 0 && CurrentUsageCount >= MaxUsageCount)
            errors.Add(new(OrderDomainErrorCode.CouponUsageLimitReached, nameof(MaxUsageCount)));

        // Check per-user usage limit
        if (MaxUsagePerUser > 0)
        {
            var userUsageCount = _usages.Count(u => u.UserId == userId);
            if (userUsageCount >= MaxUsagePerUser)
                errors.Add(new(OrderDomainErrorCode.CouponUserLimitReached, nameof(MaxUsagePerUser)));
        }

        // Check product applicability
        if (_applicableProductIds.Count != 0)
        {
            if (productIds == null || !productIds.Any())
            {
                errors.Add(new(OrderDomainErrorCode.CouponProductNotApplicable, nameof(ApplicableProductIds)));
            }
            else
            {
                var hasApplicableProduct = productIds.Any(p => _applicableProductIds.Contains(p));
                if (!hasApplicableProduct)
                    errors.Add(new(OrderDomainErrorCode.CouponProductNotApplicable, nameof(ApplicableProductIds)));
            }
        }

        // Check category applicability
        if (_applicableCategoryIds.Count != 0)
        {
            if (categoryIds == null || !categoryIds.Any())
            {
                errors.Add(new(OrderDomainErrorCode.CouponProductNotApplicable, nameof(ApplicableCategoryIds)));
            }
            else
            {
                var hasApplicableCategory = categoryIds.Any(c => _applicableCategoryIds.Contains(c));
                if (!hasApplicableCategory)
                    errors.Add(new(OrderDomainErrorCode.CouponProductNotApplicable, nameof(ApplicableCategoryIds)));
            }
        }

        // Check store applicability
        if (StoreId.HasValue)
        {
            if (!storeId.HasValue)
            {
                errors.Add(new(OrderDomainErrorCode.CouponStoreNotApplicable, nameof(StoreId)));
            }
            else if (StoreId.Value != storeId.Value)
            {
                errors.Add(new(OrderDomainErrorCode.CouponStoreNotApplicable, nameof(StoreId)));
            }
        }
        
        // Store Owner Check - currently handled dynamically or via applicable store IDs
        // if (OwnerType == CouponOwnerType.Store)
        // {
        //     if (storeId == null || !_applicableStoreIds.Contains(storeId.Value))
        //     {
        //         errors.Add(new(OrderDomainErrorCode.CouponStoreNotApplicable, nameof(ApplicableStoreIds)));
        //     }
        // }

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
            var discountAmount = orderTotal * (DiscountPercentage!.Value / 100m);
            
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

        if (MaxUsageCount > 0 && CurrentUsageCount >= MaxUsageCount)
             throw new InvalidFieldException(OrderDomainErrorCode.CouponUsageLimitReached, nameof(MaxUsageCount));

        if (MaxUsagePerUser > 0)
        {
            var userUsageCount = _usages.Count(u => u.UserId == userId);
            if (userUsageCount >= MaxUsagePerUser)
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
        IsActive = true;
    }

    /// <summary>
    /// Update coupon details based on current status
    /// </summary>
    public void Update(
        string name,
        string code,
        DateTimeOffset startDateTime,
        DateTimeOffset endDateTime,
        DateTimeOffset? earlySaveDateTime,
        int maxUsageCount,
        Money? discountAmount = null,
        decimal? discountPercentage = null,
        Money? maxDiscountAmount = null,
        Money? minOrderAmount = null,
        IEnumerable<long>? applicableProductIds = null)
    {
        if (this.IsExpired())
             throw new InvalidFieldException(OrderDomainErrorCode.CouponCannotUpdateExpired, nameof(Coupon));

        var now = DateTimeOffset.UtcNow;
        var isUpcoming = StartDateTime > now;

        if (isUpcoming)
        {
            UpdateName(name);
            UpdateCode(code);
            UpdateStartDateTime(startDateTime);
            UpdateEndDateTime(endDateTime);
            UpdateEarlySaveDateTime(earlySaveDateTime);
            
            if (maxUsageCount > 0) UpdateMaxUsageCount(maxUsageCount);

            // Update Discount Amounts
            if (DiscountType == DiscountType.FixedAmount)
            {
                if (discountAmount is null || discountAmount.Amount <= 0)
                    throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidDiscountAmount, nameof(discountAmount));
                DiscountAmount = discountAmount;
            }
            else
            {
                if (discountPercentage is null || discountPercentage <= 0 || discountPercentage > 100)
                    throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidPercentage, nameof(discountPercentage));
                DiscountPercentage = discountPercentage.Value;
                
                if (maxDiscountAmount != null && minOrderAmount != null && 
                    maxDiscountAmount.Amount < (minOrderAmount.Amount * (discountPercentage.Value / 100m)))
                {
                    throw new InvalidFieldException(OrderDomainErrorCode.CouponMaxDiscountTooSmall, nameof(maxDiscountAmount));
                }
                
                MaxDiscountAmount = maxDiscountAmount;
            }

            MinOrderAmount = minOrderAmount ?? Money.Zero();

            // Update Applicable Products if any
            if (applicableProductIds != null)
            {
                LimitToProducts(applicableProductIds);
            }
        }
        else // Ongoing
        {
            UpdateName(name);
            UpdateCode(code);
            UpdateEndDateTime(endDateTime);
            
            if (maxUsageCount > 0) UpdateMaxUsageCount(maxUsageCount);

            UpdateEarlySaveDateTime(earlySaveDateTime);
        }
    }

    /// <summary>
    /// Deactivate coupon
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new CouponDeactivatedDomainEvent(Id, Code));
    }

    /// <summary>
    /// End coupon (only for ongoing coupons)
    /// </summary>
    public void End()
    {
        var now = DateTimeOffset.UtcNow;
        if (!IsCurrentlyValid())
        {
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidStatus, nameof(Coupon));
        }

        EndDateTime = now;
    }

    /// <summary>
    /// Update name
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidName, nameof(name));
        Name = name;
    }

    /// <summary>
    /// Update code
    /// </summary>
    public void UpdateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidName, nameof(code));
        Code = code.ToUpperInvariant();
    }

    /// <summary>
    /// Update start date time (only allowed when upcoming)
    /// </summary>
    public void UpdateStartDateTime(DateTimeOffset newStartDateTime)
    {
        var now = DateTimeOffset.UtcNow;
        if (StartDateTime <= now)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponCannotUpdateOngoingStart, nameof(StartDateTime)); // Coupon is already ongoing or expired

        if (newStartDateTime <= now)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponStartTimeInPast, nameof(StartDateTime)); // New time must be in the future

        if (newStartDateTime >= EndDateTime)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponEndTimeBeforeStart, nameof(StartDateTime)); // Start must be before end

        // If it had an early save time that is now after the new start time, invalidate the early save time or throw
        if (EarlySaveDateTime.HasValue && EarlySaveDateTime.Value >= newStartDateTime)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponEarlySaveAfterStart, nameof(StartDateTime));

        StartDateTime = newStartDateTime;
    }

    /// <summary>
    /// Update end date time (allowed for upcoming and ongoing)
    /// </summary>
    public void UpdateEndDateTime(DateTimeOffset newEndDateTime)
    {
        var now = DateTimeOffset.UtcNow;
        if (this.IsExpired())
            throw new InvalidFieldException(OrderDomainErrorCode.CouponCannotUpdateExpired, nameof(EndDateTime)); // Coupon is already expired

        if (newEndDateTime <= now)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidDates, nameof(EndDateTime)); // New time must be in the future (using generic Dates or specific if needed, but Dates is ORD10004 which is fine)
            // Let's use generic dates here or add one more if we want to be super specific. 
            // Actually, I'll stick to what I have or use the existing ones if they fit.
            // ORD10004 is "Coupon dates are invalid".
            // Let's keep ORD10004 for general date errors.

        if (newEndDateTime <= StartDateTime)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponEndTimeBeforeStart, nameof(EndDateTime)); // End must be after start

        EndDateTime = newEndDateTime;
    }

    /// <summary>
    /// Update early save time (only allowed when it hasn't started yet or in upcoming)
    /// </summary>
    public void UpdateEarlySaveDateTime(DateTimeOffset? newEarlySaveDateTime)
    {
        if (EarlySaveDateTime == newEarlySaveDateTime)
            return;

        var now = DateTimeOffset.UtcNow;

        // If it was already displaying (EarlySaveDateTime was in the past), we can't change it
        if (EarlySaveDateTime.HasValue && EarlySaveDateTime.Value <= now)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponEarlySaveAlreadyStarted, nameof(EarlySaveDateTime));

        if (newEarlySaveDateTime.HasValue)
        {
            if (newEarlySaveDateTime.Value >= StartDateTime)
                throw new InvalidFieldException(OrderDomainErrorCode.CouponEarlySaveAfterStart, nameof(EarlySaveDateTime)); // Must be before start time
        }

        EarlySaveDateTime = newEarlySaveDateTime;
    }

    /// <summary>
    /// Update max usage count
    /// </summary>
    public void UpdateMaxUsageCount(int maxUsageCount)
    {
        if (maxUsageCount < CurrentUsageCount)
             throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidMaxUsage, nameof(maxUsageCount));
        
        MaxUsageCount = maxUsageCount;
    }

    /// <summary>
    /// Check if coupon is expired
    /// </summary>
    public bool IsExpired() => DateTimeOffset.UtcNow > EndDateTime || !IsActive;

    /// <summary>
    /// Check if coupon is currently valid (active and within date range)
    /// </summary>
    public bool IsCurrentlyValid()
    {
        var now = DateTimeOffset.UtcNow;
        return IsActive && now >= StartDateTime && now <= EndDateTime;
    }

    /// <summary>
    /// Get remaining usage count
    /// </summary>
    public int? GetRemainingUsage()
    {
        if (MaxUsageCount <= 0)
            return null; // Unlimited
        
        return MaxUsageCount - CurrentUsageCount;
    }
}

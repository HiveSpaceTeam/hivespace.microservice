using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record SelectedCartStoreLineSnapshot(
    long ProductId,
    long LineSubtotal
);

public record SelectedCartStoreSnapshot(
    Guid StoreId,
    string? StoreName,
    string Currency,
    long Subtotal,
    long ShippingFee,
    List<long> ProductIds,
    List<SelectedCartStoreLineSnapshot> Lines
);

public record CouponEvaluationResult(
    bool IsApplicable,
    long DiscountAmount,
    long ItemDiscount,
    long ShippingDiscount,
    long EligibleSubtotal,
    IReadOnlyList<long> EligibleProductIds,
    IReadOnlyList<DomainErrorCode> Errors
);

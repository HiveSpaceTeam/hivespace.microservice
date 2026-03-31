using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.OrderService.Domain.Exceptions;

public class OrderDomainErrorCode : DomainErrorCode
{
    private OrderDomainErrorCode(int id, string name, string code)
        : base(id, name, code) { }

    // DeliveryAddress errors (ORD1xxx)
    public static readonly OrderDomainErrorCode AddressRecipientRequired =
        new(1001, "AddressRecipientRequired", "ORD1001");
    public static readonly OrderDomainErrorCode AddressPhoneRequired =
        new(1002, "AddressPhoneRequired", "ORD1002");
    public static readonly OrderDomainErrorCode AddressStreetRequired =
        new(1003, "AddressStreetRequired", "ORD1003");
    public static readonly OrderDomainErrorCode AddressCommuneRequired =
        new(1004, "AddressCommuneRequired", "ORD1004");
    public static readonly OrderDomainErrorCode AddressProvinceRequired =
        new(1005, "AddressProvinceRequired", "ORD1005");

    // PackageDimensions errors (ORD3xxx)
    public static readonly OrderDomainErrorCode DimensionsInvalidWidth =
        new(3001, "DimensionsInvalidWidth", "ORD3001");
    public static readonly OrderDomainErrorCode DimensionsInvalidHeight =
        new(3002, "DimensionsInvalidHeight", "ORD3002");
    public static readonly OrderDomainErrorCode DimensionsInvalidLength =
        new(3003, "DimensionsInvalidLength", "ORD3003");
    public static readonly OrderDomainErrorCode DimensionsInvalidWeight =
        new(3004, "DimensionsInvalidWeight", "ORD3004");
    public static readonly OrderDomainErrorCode DimensionsInvalidExtras =
        new(3005, "DimensionsInvalidExtras", "ORD3005");

    // PhoneNumber errors (ORD4xxx)
    public static readonly OrderDomainErrorCode PhoneRequired =
        new(4001, "PhoneRequired", "ORD4001");
    public static readonly OrderDomainErrorCode PhoneInvalidFormat =
        new(4002, "PhoneInvalidFormat", "ORD4002");

    // ProductSnapshot errors (ORD5xxx)
    public static readonly OrderDomainErrorCode SnapshotInvalidProductId =
        new(5001, "SnapshotInvalidProductId", "ORD5001");
    public static readonly OrderDomainErrorCode SnapshotInvalidSkuId =
        new(5002, "SnapshotInvalidSkuId", "ORD5002");
    public static readonly OrderDomainErrorCode SnapshotProductNameRequired =
        new(5003, "SnapshotProductNameRequired", "ORD5003");
    public static readonly OrderDomainErrorCode SnapshotPriceRequired =
        new(5004, "SnapshotPriceRequired", "ORD5004");

    // OrderItem errors (ORD6xxx)
    public static readonly OrderDomainErrorCode InvalidQuantity =
        new(6001, "InvalidQuantity", "ORD6001");
    public static readonly OrderDomainErrorCode InvalidPrice =
        new(6002, "InvalidPrice", "ORD6002");

    // Order aggregate errors (ORD7xxx)
    public static readonly OrderDomainErrorCode OrderUserRequired =
        new(7001, "OrderUserRequired", "ORD7001");
    public static readonly OrderDomainErrorCode OrderAddressRequired =
        new(7002, "OrderAddressRequired", "ORD7002");
    public static readonly OrderDomainErrorCode OrderStoreIdRequired =
        new(7003, "OrderStoreIdRequired", "ORD7003");
    public static readonly OrderDomainErrorCode OrderInvalidStatus =
        new(7004, "OrderInvalidStatus", "ORD7004");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForPayment =
        new(7005, "OrderInvalidStatusForPayment", "ORD7005");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForCOD =
        new(7006, "OrderInvalidStatusForCOD", "ORD7006");
    public static readonly OrderDomainErrorCode OrderExceedsCODLimit =
        new(7007, "OrderExceedsCODLimit", "ORD7007");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForConfirmation =
        new(7008, "OrderInvalidStatusForConfirmation", "ORD7008");
    public static readonly OrderDomainErrorCode OrderNoItems =
        new(7009, "OrderNoItems", "ORD7009");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForRejection =
        new(7010, "OrderInvalidStatusForRejection", "ORD7010");
    public static readonly OrderDomainErrorCode OrderRejectionReasonRequired =
        new(7011, "OrderRejectionReasonRequired", "ORD7011");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForCompletion =
        new(7012, "OrderInvalidStatusForCompletion", "ORD7012");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForCancellation =
        new(7013, "OrderInvalidStatusForCancellation", "ORD7013");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForExpiration =
        new(7014, "OrderInvalidStatusForExpiration", "ORD7014");
    public static readonly OrderDomainErrorCode OrderNotFound =
        new(7015, "OrderNotFound", "ORD7015");
    public static readonly OrderDomainErrorCode NotOrderOwner =
        new(7016, "NotOrderOwner", "ORD7016");
    public static readonly OrderDomainErrorCode SellerStoreRequired =
        new(7017, "SellerStoreRequired", "ORD7017");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForShipping =
        new(7018, "OrderInvalidStatusForShipping", "ORD7018");
    public static readonly OrderDomainErrorCode OrderShippingIdRequired =
        new(7019, "OrderShippingIdRequired", "ORD7019");
    public static readonly OrderDomainErrorCode OrderMissingShipping =
        new(7020, "OrderMissingShipping", "ORD7020");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForDelivery =
        new(7021, "OrderInvalidStatusForDelivery", "ORD7021");
    public static readonly OrderDomainErrorCode OrderInvalidShippingFee =
        new(7022, "OrderInvalidShippingFee", "ORD7022");

    // Cart aggregate errors (ORD9xxx)
    public static readonly OrderDomainErrorCode CartUserIdRequired =
        new(9001, "CartUserIdRequired", "ORD9001");
    public static readonly OrderDomainErrorCode CartProductIdRequired =
        new(9002, "CartProductIdRequired", "ORD9002");
    public static readonly OrderDomainErrorCode CartSkuIdRequired =
        new(9003, "CartSkuIdRequired", "ORD9003");
    public static readonly OrderDomainErrorCode CartInvalidQuantity =
        new(9004, "CartInvalidQuantity", "ORD9004");
    public static readonly OrderDomainErrorCode CartItemNotFound =
        new(9005, "CartItemNotFound", "ORD9005");
    public static readonly OrderDomainErrorCode CartEmpty =
        new(9006, "CartEmpty", "ORD9006");
    public static readonly OrderDomainErrorCode CartNotFound =
        new(9007, "CartNotFound", "ORD9007");
    public static readonly OrderDomainErrorCode CartSkuNotBelongToProduct =
        new(9008, "CartSkuNotBelongToProduct", "ORD9008");

    // Coupon aggregate errors (ORD10xxx)
    public static readonly OrderDomainErrorCode CouponInvalidDiscountAmount =
        new(10003, "CouponInvalidDiscountAmount", "ORD10003");
    public static readonly OrderDomainErrorCode CouponInvalidDates =
        new(10004, "CouponInvalidDates", "ORD10004");
    public static readonly OrderDomainErrorCode CouponInvalidPercentage =
        new(10005, "CouponInvalidPercentage", "ORD10005");
    public static readonly OrderDomainErrorCode CouponInvalidMaxUsage =
        new(10006, "CouponInvalidMaxUsage", "ORD10006");
    public static readonly OrderDomainErrorCode CouponInvalidMaxUsagePerUser =
        new(10007, "CouponInvalidMaxUsagePerUser", "ORD10007");
    public static readonly OrderDomainErrorCode CouponInvalidExtension =
        new(10008, "CouponInvalidExtension", "ORD10008");
    public static readonly OrderDomainErrorCode CouponNotActive =
        new(10009, "CouponNotActive", "ORD10009");
    public static readonly OrderDomainErrorCode CouponExpired =
        new(10010, "CouponExpired", "ORD10010");
    public static readonly OrderDomainErrorCode CouponMinOrderAmountNotMet =
        new(10011, "CouponMinOrderAmountNotMet", "ORD10011");
    public static readonly OrderDomainErrorCode CouponUsageLimitReached =
        new(10012, "CouponUsageLimitReached", "ORD10012");
    public static readonly OrderDomainErrorCode CouponUserLimitReached =
        new(10013, "CouponUserLimitReached", "ORD10013");
    public static readonly OrderDomainErrorCode CouponProductNotApplicable =
        new(10014, "CouponProductNotApplicable", "ORD10014");
    public static readonly OrderDomainErrorCode CouponStoreNotApplicable =
        new(10015, "CouponStoreNotApplicable", "ORD10015");
    public static readonly OrderDomainErrorCode CouponInvalid =
        new(10016, "CouponInvalid", "ORD10016");
    public static readonly OrderDomainErrorCode CouponCodeInvalidPrefix =
        new(10017, "CouponCodeInvalidPrefix", "ORD10017");
    public static readonly OrderDomainErrorCode CouponNotStoreOwned =
        new(10018, "CouponNotStoreOwned", "ORD10018");
    public static readonly OrderDomainErrorCode CouponInvalidStatus =
        new(10019, "CouponInvalidStatus", "ORD10019");
    public static readonly OrderDomainErrorCode CouponCodeInvalidLength =
        new(10020, "CouponCodeInvalidLength", "ORD10020");
    public static readonly OrderDomainErrorCode CouponNameInvalidLength =
        new(10021, "CouponNameInvalidLength", "ORD10021");
    public static readonly OrderDomainErrorCode CouponInvalidMinOrderAmount =
        new(10022, "CouponInvalidMinOrderAmount", "ORD10022");
    public static readonly OrderDomainErrorCode CouponInvalidUsageLimit =
        new(10023, "CouponInvalidUsageLimit", "ORD10023");
    public static readonly OrderDomainErrorCode CouponInvalidUsageLimitPerUser =
        new(10024, "CouponInvalidUsageLimitPerUser", "ORD10024");
    public static readonly OrderDomainErrorCode CouponNotFound =
        new(10025, "CouponNotFound", "ORD10025");
    public static readonly OrderDomainErrorCode CouponInvalidName =
        new(10026, "CouponInvalidName", "ORD10026");
    public static readonly OrderDomainErrorCode CouponMaxDiscountTooSmall =
        new(10027, "CouponMaxDiscountTooSmall", "ORD10027");
    public static readonly OrderDomainErrorCode CouponEarlySaveAfterStart =
        new(10028, "CouponEarlySaveAfterStart", "ORD10028");
    public static readonly OrderDomainErrorCode CouponStartTimeInPast =
        new(10030, "CouponStartTimeInPast", "ORD10030");
    public static readonly OrderDomainErrorCode CouponEndTimeBeforeStart =
        new(10031, "CouponEndTimeBeforeStart", "ORD10031");
    public static readonly OrderDomainErrorCode CouponCannotUpdateOngoingStart =
        new(10032, "CouponCannotUpdateOngoingStart", "ORD10032");
    public static readonly OrderDomainErrorCode CouponCannotUpdateExpired =
        new(10033, "CouponCannotUpdateExpired", "ORD10033");
    public static readonly OrderDomainErrorCode CouponEarlySaveAlreadyStarted =
        new(10034, "CouponEarlySaveAlreadyStarted", "ORD10034");

    // Checkout saga failure errors (ORD11xxx)
    public static readonly OrderDomainErrorCode CheckoutValidationFailed =
        new(11001, "CheckoutValidationFailed", "ORD11001");
    public static readonly OrderDomainErrorCode CheckoutInventoryUnavailable =
        new(11002, "CheckoutInventoryUnavailable", "ORD11002");
    public static readonly OrderDomainErrorCode CheckoutCODLimitExceeded =
        new(11003, "CheckoutCODLimitExceeded", "ORD11003");
    public static readonly OrderDomainErrorCode CheckoutTimeout =
        new(11004, "CheckoutTimeout", "ORD11004");
    public static readonly OrderDomainErrorCode CheckoutInternalError =
        new(11005, "CheckoutInternalError", "ORD11005");
}

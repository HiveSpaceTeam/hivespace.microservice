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
    public static readonly OrderDomainErrorCode AddressWardRequired =
        new(1004, "AddressWardRequired", "ORD1004");
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
    public static readonly OrderDomainErrorCode OrderPackageNull =
        new(7003, "OrderPackageNull", "ORD7003");
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
    public static readonly OrderDomainErrorCode OrderPackagesNotConfirmed =
        new(7009, "OrderPackagesNotConfirmed", "ORD7009");
    public static readonly OrderDomainErrorCode OrderPackageNotFound =
        new(7010, "OrderPackageNotFound", "ORD7010");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForCompletion =
        new(7011, "OrderInvalidStatusForCompletion", "ORD7011");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForCancellation =
        new(7012, "OrderInvalidStatusForCancellation", "ORD7012");
    public static readonly OrderDomainErrorCode OrderInvalidStatusForExpiration =
        new(7013, "OrderInvalidStatusForExpiration", "ORD7013");

    // OrderPackage errors (ORD8xxx)
    public static readonly OrderDomainErrorCode PackageStoreIdRequired =
        new(8001, "PackageStoreIdRequired", "ORD8001");
    public static readonly OrderDomainErrorCode PackageBuyerIdRequired =
        new(8002, "PackageBuyerIdRequired", "ORD8002");
    public static readonly OrderDomainErrorCode PackageInvalidStatusForAddItem =
        new(8003, "PackageInvalidStatusForAddItem", "ORD8003");
    public static readonly OrderDomainErrorCode PackageInvalidStatusForDiscount =
        new(8004, "PackageInvalidStatusForDiscount", "ORD8004");
    public static readonly OrderDomainErrorCode PackageInvalidShippingFee =
        new(8005, "PackageInvalidShippingFee", "ORD8005");
    public static readonly OrderDomainErrorCode PackageInvalidStatusForConfirmation =
        new(8006, "PackageInvalidStatusForConfirmation", "ORD8006");
    public static readonly OrderDomainErrorCode PackageNoItems =
        new(8007, "PackageNoItems", "ORD8007");
    public static readonly OrderDomainErrorCode PackageInvalidStatusForRejection =
        new(8008, "PackageInvalidStatusForRejection", "ORD8008");
    public static readonly OrderDomainErrorCode PackageRejectionReasonRequired =
        new(8009, "PackageRejectionReasonRequired", "ORD8009");
    public static readonly OrderDomainErrorCode PackageInvalidStatusForShipping =
        new(8010, "PackageInvalidStatusForShipping", "ORD8010");
    public static readonly OrderDomainErrorCode PackageShippingIdRequired =
        new(8011, "PackageShippingIdRequired", "ORD8011");
    public static readonly OrderDomainErrorCode PackageMissingShipping =
        new(8012, "PackageMissingShipping", "ORD8012");
    public static readonly OrderDomainErrorCode PackageInvalidStatusForDelivery =
        new(8013, "PackageInvalidStatusForDelivery", "ORD8013");
    public static readonly OrderDomainErrorCode PackageInvalidStatusForCompletion =
        new(8014, "PackageInvalidStatusForCompletion", "ORD8014");
    public static readonly OrderDomainErrorCode PackageInvalidStatusForCancellation =
        new(8015, "PackageInvalidStatusForCancellation", "ORD8015");

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

    // Coupon aggregate errors (ORD10xxx)
    public static readonly OrderDomainErrorCode CouponCodeRequired =
        new(10001, "CouponCodeRequired", "ORD10001");
    public static readonly OrderDomainErrorCode CouponNameRequired =
        new(10002, "CouponNameRequired", "ORD10002");
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
}

using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.PaymentService.Domain.Exceptions;

public class PaymentDomainErrorCode : DomainErrorCode
{
    private PaymentDomainErrorCode(int id, string name, string code)
        : base(id, name, code) { }

    // Payment aggregate errors (PAY1xxx)
    public static readonly PaymentDomainErrorCode PaymentNotFound =
        new(1001, "PaymentNotFound", "PAY1001");
    public static readonly PaymentDomainErrorCode PaymentInvalidStatus =
        new(1002, "PaymentInvalidStatus", "PAY1002");
    public static readonly PaymentDomainErrorCode PaymentExpired =
        new(1003, "PaymentExpired", "PAY1003");
    public static readonly PaymentDomainErrorCode PaymentAlreadySucceeded =
        new(1004, "PaymentAlreadySucceeded", "PAY1004");
    public static readonly PaymentDomainErrorCode PaymentOrderIdRequired =
        new(1005, "PaymentOrderIdRequired", "PAY1005");
    public static readonly PaymentDomainErrorCode PaymentBuyerIdRequired =
        new(1006, "PaymentBuyerIdRequired", "PAY1006");
    public static readonly PaymentDomainErrorCode PaymentAmountRequired =
        new(1007, "PaymentAmountRequired", "PAY1007");
    public static readonly PaymentDomainErrorCode PaymentIdempotencyKeyRequired =
        new(1008, "PaymentIdempotencyKeyRequired", "PAY1008");
    public static readonly PaymentDomainErrorCode PaymentAccessForbidden =
        new(1009, "PaymentAccessForbidden", "PAY1009");
    public static readonly PaymentDomainErrorCode PaymentReferenceNoRequired =
        new(1010, "PaymentReferenceNoRequired", "PAY1010");
    public static readonly PaymentDomainErrorCode PaymentLinkedOrdersRequired =
        new(1011, "PaymentLinkedOrdersRequired", "PAY1011");
    public static readonly PaymentDomainErrorCode PaymentLinkedOrderTotalMismatch =
        new(1012, "PaymentLinkedOrderTotalMismatch", "PAY1012");
    public static readonly PaymentDomainErrorCode PaymentLinkedOrderDuplicate =
        new(1013, "PaymentLinkedOrderDuplicate", "PAY1013");
    public static readonly PaymentDomainErrorCode PaymentMethodInvalid =
        new(1014, "PaymentMethodInvalid", "PAY1014");
    public static readonly PaymentDomainErrorCode PaymentRetryNotAllowed =
        new(1015, "PaymentRetryNotAllowed", "PAY1015");

    // Wallet aggregate errors (PAY2xxx)
    public static readonly PaymentDomainErrorCode WalletNotFound =
        new(2001, "WalletNotFound", "PAY2001");
    public static readonly PaymentDomainErrorCode WalletInactive =
        new(2002, "WalletInactive", "PAY2002");
    public static readonly PaymentDomainErrorCode WalletInsufficientBalance =
        new(2003, "WalletInsufficientBalance", "PAY2003");
    public static readonly PaymentDomainErrorCode WalletUserIdRequired =
        new(2004, "WalletUserIdRequired", "PAY2004");
    public static readonly PaymentDomainErrorCode WalletInvalidAmount =
        new(2005, "WalletInvalidAmount", "PAY2005");

    // BankAccount value object errors (PAY2xxx continued)
    public static readonly PaymentDomainErrorCode BankAccountBankCodeRequired =
        new(2006, "BankAccountBankCodeRequired", "PAY2006");
    public static readonly PaymentDomainErrorCode BankAccountNumberRequired =
        new(2007, "BankAccountNumberRequired", "PAY2007");
    public static readonly PaymentDomainErrorCode BankAccountHolderNameRequired =
        new(2008, "BankAccountHolderNameRequired", "PAY2008");

    // Gateway errors (PAY3xxx)
    public static readonly PaymentDomainErrorCode InvalidGatewaySignature =
        new(3001, "InvalidGatewaySignature", "PAY3001");
    public static readonly PaymentDomainErrorCode GatewayNotSupported =
        new(3002, "GatewayNotSupported", "PAY3002");
    public static readonly PaymentDomainErrorCode GatewayInitiationFailed =
        new(3003, "GatewayInitiationFailed", "PAY3003");
    public static readonly PaymentDomainErrorCode PlatformCurrencyPolicyMissing =
        new(3004, "PlatformCurrencyPolicyMissing", "PAY3004");
    public static readonly PaymentDomainErrorCode PlatformCurrencyDisabled =
        new(3005, "PlatformCurrencyDisabled", "PAY3005");
}

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

    // Wallet aggregate errors (PAY2xxx)
    public static readonly PaymentDomainErrorCode WalletNotFound =
        new(2001, "WalletNotFound", "PAY2001");
    public static readonly PaymentDomainErrorCode WalletInactive =
        new(2002, "WalletInactive", "PAY2002");
    public static readonly PaymentDomainErrorCode WalletInsufficientBalance =
        new(2003, "WalletInsufficientBalance", "PAY2003");
    public static readonly PaymentDomainErrorCode WalletUserIdRequired =
        new(2004, "WalletUserIdRequired", "PAY2004");

    // Gateway errors (PAY3xxx)
    public static readonly PaymentDomainErrorCode InvalidGatewaySignature =
        new(3001, "InvalidGatewaySignature", "PAY3001");
    public static readonly PaymentDomainErrorCode GatewayNotSupported =
        new(3002, "GatewayNotSupported", "PAY3002");
    public static readonly PaymentDomainErrorCode GatewayInitiationFailed =
        new(3003, "GatewayInitiationFailed", "PAY3003");
}

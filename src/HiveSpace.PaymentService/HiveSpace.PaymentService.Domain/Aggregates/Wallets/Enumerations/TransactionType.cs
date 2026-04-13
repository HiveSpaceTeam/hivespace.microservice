namespace HiveSpace.PaymentService.Domain.Aggregates.Wallets.Enumerations;

public enum TransactionType
{
    Payment,
    Refund,
    Withdrawal,
    Escrow,
    Adjustment
}

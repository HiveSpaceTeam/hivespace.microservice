using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.PaymentService.Domain.ValueObjects;

// Phase 3 — used for seller withdrawal payouts
public class BankAccount : ValueObject
{
    public string BankCode { get; private set; } = null!;
    public string AccountNumber { get; private set; } = null!;
    public string AccountHolderName { get; private set; } = null!;

    private BankAccount() { }

    public static BankAccount Create(string bankCode, string accountNumber, string accountHolderName)
        => new() { BankCode = bankCode, AccountNumber = accountNumber, AccountHolderName = accountHolderName };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BankCode;
        yield return AccountNumber;
        yield return AccountHolderName;
    }
}

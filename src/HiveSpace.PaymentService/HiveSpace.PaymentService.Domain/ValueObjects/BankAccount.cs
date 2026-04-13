using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Domain.Exceptions;

namespace HiveSpace.PaymentService.Domain.ValueObjects;

// Phase 3 — used for seller withdrawal payouts
public class BankAccount : ValueObject
{
    public string BankCode { get; private set; } = null!;
    public string AccountNumber { get; private set; } = null!;
    public string AccountHolderName { get; private set; } = null!;

    private BankAccount() { }

    public static BankAccount Create(string bankCode, string accountNumber, string accountHolderName)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
            throw new InvalidFieldException(PaymentDomainErrorCode.BankAccountBankCodeRequired, nameof(bankCode));
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new InvalidFieldException(PaymentDomainErrorCode.BankAccountNumberRequired, nameof(accountNumber));
        if (string.IsNullOrWhiteSpace(accountHolderName))
            throw new InvalidFieldException(PaymentDomainErrorCode.BankAccountHolderNameRequired, nameof(accountHolderName));

        return new() { BankCode = bankCode, AccountNumber = accountNumber, AccountHolderName = accountHolderName };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BankCode;
        yield return AccountNumber;
        yield return AccountHolderName;
    }
}

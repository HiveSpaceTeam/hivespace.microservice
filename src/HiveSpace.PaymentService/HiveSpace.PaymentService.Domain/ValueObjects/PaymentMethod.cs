using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.PaymentService.Domain.ValueObjects;

public class PaymentMethod : ValueObject
{
    public PaymentMethodType Type { get; private set; }
    public string? CardLast4 { get; private set; }
    public string? CardBrand { get; private set; }
    public string? WalletProvider { get; private set; }
    public string? BankCode { get; private set; }

    private PaymentMethod() { }

    public static PaymentMethod CreditCard(string last4, string brand)
        => new() { Type = PaymentMethodType.CreditCard, CardLast4 = last4, CardBrand = brand };

    public static PaymentMethod EWallet(string provider)
        => new() { Type = PaymentMethodType.EWallet, WalletProvider = provider };

    public static PaymentMethod BankTransfer(string bankCode)
        => new() { Type = PaymentMethodType.BankTransfer, BankCode = bankCode };

    public static PaymentMethod COD()
        => new() { Type = PaymentMethodType.COD };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return CardLast4 ?? string.Empty;
        yield return CardBrand ?? string.Empty;
        yield return WalletProvider ?? string.Empty;
        yield return BankCode ?? string.Empty;
    }
}

public enum PaymentMethodType
{
    CreditCard,
    DebitCard,
    EWallet,
    BankTransfer,
    COD
}

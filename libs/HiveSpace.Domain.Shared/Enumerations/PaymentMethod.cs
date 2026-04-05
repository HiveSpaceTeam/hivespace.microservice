using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.Domain.Shared.Enumerations;

public class PaymentMethod(int id, string name) : Enumeration(id, name)
{
    public static readonly PaymentMethod VNPAY = new(1, nameof(VNPAY));
    public static readonly PaymentMethod MOMO = new(3, nameof(MOMO));
    public static readonly PaymentMethod BankTransfer = new(4, nameof(BankTransfer));
    public static readonly PaymentMethod Balance = new(5, nameof(Balance));
    public static readonly PaymentMethod COD = new(6, nameof(COD));
    public static readonly PaymentMethod PayPal = new(7, nameof(PayPal));

    public bool IsOnlineGateway() =>
        this == VNPAY || this == MOMO || this == PayPal;

    public bool RequiresBalance() => this == Balance;

    public bool IsCashOnDelivery() => this == COD;
}
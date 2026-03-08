using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.OrderService.Domain.Enumerations;

public class PaymentMethod(int id, string name) : Enumeration(id, name)
{
    public static readonly PaymentMethod VNPAY = new(1, nameof(VNPAY));
    public static readonly PaymentMethod ALEPAY = new(2, nameof(ALEPAY));
    public static readonly PaymentMethod MOMO = new(3, nameof(MOMO));
    public static readonly PaymentMethod BankTransfer = new(4, "BANK_TRANSFER");
    public static readonly PaymentMethod Balance = new(5, "BALANCE");
    public static readonly PaymentMethod COD = new(6, nameof(COD));
    public static readonly PaymentMethod PayPal = new(7, "PAYPAL");
    public static readonly PaymentMethod PhoneCard = new(8, "PHONE_CARD");

    public bool IsOnlineGateway() => 
        this == VNPAY || this == ALEPAY || this == MOMO || this == PayPal;
    
    public bool RequiresBalance() => this == Balance;
    
    public bool IsCashOnDelivery() => this == COD;
}

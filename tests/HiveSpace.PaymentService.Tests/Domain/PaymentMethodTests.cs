using FluentAssertions;
using HiveSpace.PaymentService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Domain;

public class PaymentMethodTests
{
    [Fact]
    public void Create_WithValidGateway_Succeeds()
    {
        var method = PaymentMethod.BankTransfer("VNPAY");

        method.Should().NotBeNull();
        method.Type.Should().Be(PaymentMethodType.BankTransfer);
        method.BankCode.Should().Be("VNPAY");
    }

    [Fact]
    public void TwoPaymentMethodsWithSameGateway_AreEqual()
    {
        var first = PaymentMethod.BankTransfer("VNPAY");
        var second = PaymentMethod.BankTransfer("VNPAY");

        first.Should().Be(second);
    }

    [Fact]
    public void TwoPaymentMethodsWithDifferentGateways_AreNotEqual()
    {
        var bankTransfer = PaymentMethod.BankTransfer("VNPAY");
        var eWallet = PaymentMethod.EWallet("MOMO");

        bankTransfer.Should().NotBe(eWallet);
    }

    [Fact]
    public void Create_CreditCard_SetsCardFields()
    {
        var method = PaymentMethod.CreditCard("4242", "Visa");

        method.Type.Should().Be(PaymentMethodType.CreditCard);
        method.CardLast4.Should().Be("4242");
        method.CardBrand.Should().Be("Visa");
    }

    [Fact]
    public void Create_EWallet_SetsProvider()
    {
        var method = PaymentMethod.EWallet("MOMO");

        method.Type.Should().Be(PaymentMethodType.EWallet);
        method.WalletProvider.Should().Be("MOMO");
    }

    [Fact]
    public void Create_COD_HasCorrectType()
    {
        var method = PaymentMethod.COD();

        method.Type.Should().Be(PaymentMethodType.COD);
    }

    [Fact]
    public void TwoIdenticalCreditCards_AreEqual()
    {
        var first = PaymentMethod.CreditCard("1234", "MasterCard");
        var second = PaymentMethod.CreditCard("1234", "MasterCard");

        first.Should().Be(second);
    }
}

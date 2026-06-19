using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Domain;

public class BankAccountTests
{
    [Fact]
    public void Create_WithValidData_ReturnsInstance()
    {
        var account = BankAccount.Create("VCB", "1234567890", "Nguyen Van A");

        account.BankCode.Should().Be("VCB");
        account.AccountNumber.Should().Be("1234567890");
        account.AccountHolderName.Should().Be("Nguyen Van A");
    }

    [Fact]
    public void Create_WithEmptyBankCode_ThrowsInvalidFieldException()
    {
        var act = () => BankAccount.Create("", "1234567890", "Nguyen Van A");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithWhitespaceBankCode_ThrowsInvalidFieldException()
    {
        var act = () => BankAccount.Create("   ", "1234567890", "Nguyen Van A");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithEmptyAccountNumber_ThrowsInvalidFieldException()
    {
        var act = () => BankAccount.Create("VCB", "", "Nguyen Van A");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void Create_WithEmptyAccountHolderName_ThrowsInvalidFieldException()
    {
        var act = () => BankAccount.Create("VCB", "1234567890", "");
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void TwoBankAccounts_WithSameData_AreEqual()
    {
        var a = BankAccount.Create("VCB", "1234567890", "Nguyen Van A");
        var b = BankAccount.Create("VCB", "1234567890", "Nguyen Van A");

        a.Should().Be(b);
    }

    [Fact]
    public void TwoBankAccounts_WithDifferentAccountNumber_AreNotEqual()
    {
        var a = BankAccount.Create("VCB", "1234567890", "Nguyen Van A");
        var b = BankAccount.Create("VCB", "0987654321", "Nguyen Van A");

        a.Should().NotBe(b);
    }
}

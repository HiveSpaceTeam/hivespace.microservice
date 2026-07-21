using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Api.Models;
using Xunit;

namespace HiveSpace.OrderService.Tests.Api;

public class CheckoutRequestTests
{
    [Fact]
    public void GetPaymentMethod_WithPaymentMethodCode_ResolvesCaseInsensitiveMethod()
    {
        var request = new CheckoutRequest { PaymentMethodCode = "vnpay" };

        var result = request.GetPaymentMethod();

        result.Should().Be(PaymentMethod.VNPAY);
    }

    [Fact]
    public void GetPaymentMethod_WithNumericPaymentMethod_PrefersNumericMethod()
    {
        var request = new CheckoutRequest
        {
            PaymentMethod = PaymentMethod.COD.Id,
            PaymentMethodCode = "VNPAY"
        };

        var result = request.GetPaymentMethod();

        result.Should().Be(PaymentMethod.COD);
    }

    [Fact]
    public void GetPaymentMethod_WithoutPaymentMethod_DefaultsToCOD()
    {
        var request = new CheckoutRequest();

        var result = request.GetPaymentMethod();

        result.Should().Be(PaymentMethod.COD);
    }

    [Fact]
    public void GetPaymentMethod_WithUnknownPaymentMethodCode_Throws()
    {
        var request = new CheckoutRequest { PaymentMethodCode = "UNKNOWN" };

        var act = () => request.GetPaymentMethod();

        act.Should().Throw<InvalidFieldException>();
    }
}

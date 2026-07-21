using FluentAssertions;
using HiveSpace.PaymentService.Application.Payments.Queries.GetPaymentMethods;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Application.Payments;

public class GetPaymentMethodsQueryHandlerTests
{
    [Fact]
    public async Task Handle_DefaultConfiguration_ReturnsCodVnpayAndFutureStripeInSortOrder()
    {
        var handler = new GetPaymentMethodsQueryHandler();

        var result = await handler.Handle(new GetPaymentMethodsQuery(), CancellationToken.None);

        result.Methods.Should().HaveCount(3);
        result.Methods.Select(x => x.SortOrder).Should().Equal(10, 20, 30);

        result.Methods[0].Should().BeEquivalentTo(new
        {
            Code = "COD",
            DisplayName = "Cash on delivery",
            Kind = "Offline",
            GatewayCode = (string?)null,
            IsEnabled = true,
            IsCheckoutSelectable = true,
            Availability = "Available",
            SortOrder = 10
        });

        result.Methods[1].Should().BeEquivalentTo(new
        {
            Code = "VNPAY",
            DisplayName = "VNPay",
            Kind = "Online",
            GatewayCode = "VNPAY",
            IsEnabled = true,
            IsCheckoutSelectable = true,
            Availability = "Available",
            SortOrder = 20
        });

        result.Methods[2].Should().BeEquivalentTo(new
        {
            Code = "STRIPE",
            DisplayName = "Stripe",
            Kind = "Online",
            GatewayCode = "STRIPE",
            IsEnabled = false,
            IsCheckoutSelectable = false,
            Availability = "Future",
            SortOrder = 30
        });
    }
}

using FluentAssertions;
using HiveSpace.PaymentService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Domain;

public class GatewayResponseTests
{
    [Fact]
    public void Constructor_WithSuccess_SetsProperties()
    {
        var response = new GatewayResponse("{\"status\":\"ok\"}", true);

        response.RawResponse.Should().Be("{\"status\":\"ok\"}");
        response.Success.Should().BeTrue();
        response.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithFailureAndErrorMessage_SetsErrorMessage()
    {
        var response = new GatewayResponse("{\"error\":\"timeout\"}", false, "Request timed out");

        response.Success.Should().BeFalse();
        response.ErrorMessage.Should().Be("Request timed out");
    }

    [Fact]
    public void TwoResponses_WithSameRawAndSuccess_AreEqual()
    {
        var a = new GatewayResponse("{\"status\":\"ok\"}", true);
        var b = new GatewayResponse("{\"status\":\"ok\"}", true);

        a.Should().Be(b);
    }

    [Fact]
    public void TwoResponses_DifferingOnlyByErrorMessage_AreStillEqual()
    {
        // ErrorMessage is intentionally excluded from GetEqualityComponents
        var a = new GatewayResponse("{\"status\":\"ok\"}", true, "msg1");
        var b = new GatewayResponse("{\"status\":\"ok\"}", true, "msg2");

        a.Should().Be(b);
    }

    [Fact]
    public void TwoResponses_WithDifferentSuccess_AreNotEqual()
    {
        var a = new GatewayResponse("{\"status\":\"ok\"}", true);
        var b = new GatewayResponse("{\"status\":\"ok\"}", false);

        a.Should().NotBe(b);
    }
}

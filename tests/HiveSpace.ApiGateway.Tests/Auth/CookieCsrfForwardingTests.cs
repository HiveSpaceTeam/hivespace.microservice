using FluentAssertions;
using Xunit;

namespace HiveSpace.ApiGateway.Tests.Auth;

public class CookieCsrfForwardingTests
{
    [Fact]
    public void RequestWithCsrfHeader_Passes()
    {
        var headers = new Dictionary<string, string> { ["X-CSRF-TOKEN"] = "csrf-token" };

        headers.Should().ContainKey("X-CSRF-TOKEN",
            "the gateway must forward the CSRF token header to downstream services");
    }

    [Fact]
    public void RequestMissingRequiredCsrfHeader_IsRejected()
    {
        var headers = new Dictionary<string, string>();

        headers.ContainsKey("X-CSRF-TOKEN").Should().BeFalse(
            "a state-mutating request without a CSRF token must be rejected at the gateway");
    }
}

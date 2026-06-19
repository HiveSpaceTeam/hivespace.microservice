using System.Net;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.ValueObjects;
using HiveSpace.PaymentService.Infrastructure.Gateways;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Gateways;

public class VNPayGatewayTests
{
    [Fact]
    public async Task InitiatePaymentAsync_ShouldExcludeIpnUrl_AndSignCanonicalQuery()
    {
        var gateway = CreateGateway();
        var payment = CreatePayment();
        var returnUrl = "https://merchant.test/payment/result?order=123&status=waiting";

        var result = await gateway.InitiatePaymentAsync(payment, returnUrl, "https://merchant.test/payment/cancel");
        var queryParameters = ParseQueryString(result.PaymentUrl);

        queryParameters.Should().NotContainKey("vnp_IpnUrl");
        queryParameters.Should().ContainKey("vnp_ReturnUrl").WhoseValue.Should().Be(returnUrl);

        var actualHash = queryParameters["vnp_SecureHash"];
        var signedParameters = queryParameters
            .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.Ordinal))
            .Where(kv => kv.Key is not "vnp_SecureHash" and not "vnp_SecureHashType")
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        actualHash.Should().Be(ComputeHash(BuildCanonicalData(signedParameters)));
    }

    [Fact]
    public async Task InitiatePaymentAsync_ShouldPreserveSpecialCharactersInReturnUrl()
    {
        var gateway = CreateGateway();
        var payment = CreatePayment();
        var returnUrl = "https://merchant.test/payment/result?message=thanh toan thanh cong&next=/orders/1";

        var result = await gateway.InitiatePaymentAsync(payment, returnUrl, "https://merchant.test/payment/cancel");
        var queryParameters = ParseQueryString(result.PaymentUrl);

        queryParameters["vnp_ReturnUrl"].Should().Be(returnUrl);
    }

    [Fact]
    public async Task VerifyWebhookAsync_ShouldAcceptValidSignature()
    {
        var gateway = CreateGateway();
        var payload = new Dictionary<string, string>
        {
            ["vnp_Amount"] = "1500000",
            ["vnp_Command"] = "pay",
            ["vnp_ResponseCode"] = "00",
            ["vnp_TmnCode"] = Config.TmnCode,
            ["vnp_TransactionNo"] = "12345678",
            ["vnp_TxnRef"] = Guid.NewGuid().ToString("N"),
        };

        payload["vnp_SecureHash"] = ComputeHash(BuildCanonicalData(payload));

        var result = await gateway.VerifyWebhookAsync(payload);

        result.Success.Should().BeTrue();
        result.TransactionId.Should().Be(payload["vnp_TxnRef"]);
    }

    [Fact]
    public async Task VerifyWebhookAsync_ShouldRejectModifiedSignedField()
    {
        var gateway = CreateGateway();
        var payload = new Dictionary<string, string>
        {
            ["vnp_Amount"] = "1500000",
            ["vnp_Command"] = "pay",
            ["vnp_ResponseCode"] = "00",
            ["vnp_TmnCode"] = Config.TmnCode,
            ["vnp_TxnRef"] = Guid.NewGuid().ToString("N"),
        };

        payload["vnp_SecureHash"] = ComputeHash(BuildCanonicalData(payload));
        payload["vnp_Amount"] = "1600000";

        var act = () => gateway.VerifyWebhookAsync(payload);

        await act.Should().ThrowAsync<InvalidFieldException>();
    }

    private static VNPayGateway CreateGateway()
    {
        return new VNPayGateway(Options.Create(Config), NullLogger<VNPayGateway>.Instance);
    }

    private static Payment CreatePayment()
    {
        return Payment.CreateForOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Money.FromVND(15_000),
            PaymentMethod.BankTransfer("VNPAY"),
            PaymentGateway.VNPay,
            Guid.NewGuid().ToString("N"));
    }

    private static Dictionary<string, string> ParseQueryString(string paymentUrl)
    {
        var uri = new Uri(paymentUrl);
        return uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .ToDictionary(
                pair => WebUtility.UrlDecode(pair[0]),
                pair => pair.Length > 1 ? WebUtility.UrlDecode(pair[1]) : string.Empty);
    }

    private static string BuildCanonicalData(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        return string.Join("&", parameters
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
    }

    private static string ComputeHash(string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(Config.HashSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static readonly VNPayConfiguration Config = new()
    {
        TmnCode = "28FJPQF8",
        HashSecret = "IKRTI9KSJY6KR4W5CNLSJ7OAIIKSD120",
        BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
        ReturnUrl = "https://merchant.test/payment/result",
        IpnUrl = "https://merchant.test/api/v1/payments/webhook/vnpay",
        IpAddress = "127.0.0.1",
        Version = "2.1.0",
        Command = "pay",
        CurrCode = "VND",
        Locale = "vn"
    };
}

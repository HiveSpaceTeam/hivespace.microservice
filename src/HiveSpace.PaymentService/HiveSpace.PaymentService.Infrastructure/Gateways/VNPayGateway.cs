using System.Net;
using System.Security.Cryptography;
using System.Text;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Exceptions;
using HiveSpace.PaymentService.Domain.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HiveSpace.PaymentService.Infrastructure.Gateways;

public class VNPayGateway(IOptions<VNPayConfiguration> options, ILogger<VNPayGateway> logger) : IPaymentGateway
{
    private readonly VNPayConfiguration _config = options.Value;
    private static readonly string[] ExcludedSignatureFields = ["vnp_SecureHash", "vnp_SecureHashType"];

    public PaymentGateway GatewayType => PaymentGateway.VNPay;

    public Task<GatewayInitiateResult> InitiatePaymentAsync(
        Payment payment, string returnUrl, string cancelUrl, CancellationToken ct = default)
    {
        var createDate = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)); // GMT+7 (Vietnam)
        var txnRef = payment.Id.ToString("N"); // full Guid without dashes — used to resolve payment on webhook

        var vnpParams = new SortedDictionary<string, string>
        {
            ["vnp_Version"] = _config.Version,
            ["vnp_Command"] = _config.Command,
            ["vnp_TmnCode"] = _config.TmnCode,
            ["vnp_Amount"] = (payment.Amount.Amount * 100).ToString(), // VNPay uses smallest unit * 100
            ["vnp_CurrCode"] = _config.CurrCode,
            ["vnp_TxnRef"] = txnRef,
            ["vnp_OrderInfo"] = $"Payment for order {payment.OrderId}",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = _config.Locale,
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_IpAddr"] = _config.IpAddress,
            ["vnp_CreateDate"] = createDate.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = createDate.AddMinutes(15).ToString("yyyyMMddHHmmss"),
        };

        var hashData = BuildCanonicalData(vnpParams);
        var query = BuildCanonicalData(vnpParams);
        var secureHash = ComputeHmacSha512(_config.HashSecret, hashData);
        var paymentUrl = $"{_config.BaseUrl}?{query}&vnp_SecureHash={secureHash}";

        logger.LogInformation("[VNPay] HashInput: {HashInput}", hashData);
        logger.LogInformation("[VNPay] SecureHash: {Hash}", secureHash);
        logger.LogInformation("[VNPay] PaymentUrl: {Url}", paymentUrl);

        return Task.FromResult(new GatewayInitiateResult(paymentUrl, txnRef));
    }

    public Task<GatewayVerifyResult> VerifyWebhookAsync(
        Dictionary<string, string> payload, CancellationToken ct = default)
    {
        if (!payload.TryGetValue("vnp_SecureHash", out var receivedHash))
            throw new InvalidFieldException(PaymentDomainErrorCode.InvalidGatewaySignature, "vnp_SecureHash");

        var filteredParams = payload
            .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.Ordinal))
            .Where(kv => !ExcludedSignatureFields.Contains(kv.Key, StringComparer.Ordinal))
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        // ASP.NET already URL-decodes query params — hash the raw decoded values to match VNPay's server-side computation
        var hashData = BuildCanonicalData(filteredParams);
        var expectedHash = ComputeHmacSha512(_config.HashSecret, hashData);

        logger.LogInformation("[VNPay] VerifyHashInput: {HashInput}", hashData);
        logger.LogInformation("[VNPay] VerifyExpectedHash: {Hash}", expectedHash);

        if (!string.Equals(expectedHash, receivedHash, StringComparison.OrdinalIgnoreCase))
            throw new InvalidFieldException(PaymentDomainErrorCode.InvalidGatewaySignature, "vnp_SecureHash");

        payload.TryGetValue("vnp_ResponseCode", out var responseCode);
        payload.TryGetValue("vnp_TxnRef", out var txnRef);
        var rawResponse = string.Join(";", payload.Select(kv => $"{kv.Key}={kv.Value}"));

        var success = responseCode == "00";
        var errorMessage = success ? null : $"VNPay response code: {responseCode}";

        return Task.FromResult(new GatewayVerifyResult(success, txnRef ?? string.Empty, rawResponse, errorMessage));
    }

    private static string ComputeHmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hash).ToLower();
    }

    private static string BuildCanonicalData(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        return string.Join("&", parameters
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
    }
}

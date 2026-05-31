using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace HiveSpace.Core.Helpers;

public static class BrowserSessionTokenSigner
{
    public static string Sign(string purpose, string payloadJson, string signingKey)
    {
        var payload = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var signature = ComputeSignature(purpose, payload, signingKey);
        return $"{payload}.{signature}";
    }

    public static bool TryReadPayload(string purpose, string token, string signingKey, out string payloadJson)
    {
        payloadJson = string.Empty;

        var separatorIndex = token.IndexOf('.');
        if (separatorIndex <= 0 || separatorIndex == token.Length - 1)
            return false;

        var payload = token[..separatorIndex];
        var signature = token[(separatorIndex + 1)..];
        var expectedSignature = ComputeSignature(purpose, payload, signingKey);

        if (!FixedTimeEquals(expectedSignature, signature))
            return false;

        try
        {
            payloadJson = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(payload));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string ComputeSignature(string purpose, string payload, string signingKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
        var bytes = Encoding.UTF8.GetBytes($"{purpose}.{payload}");
        return WebEncoders.Base64UrlEncode(hmac.ComputeHash(bytes));
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);
        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}

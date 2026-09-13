using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HiveSpace.UserService.Application.Stores.Commands.ProvisionImportedSellerStore;

internal sealed record ImportedSellerProfileKey(string Email, string UserName)
{
    public static ImportedSellerProfileKey Create(string sourceSystem, string externalSellerId)
    {
        var source = Normalize(sourceSystem);
        var external = Normalize(externalSellerId);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{source}:{external}")))
            .ToLowerInvariant();

        return new ImportedSellerProfileKey(
            $"imported-seller+{source}-{hash[..16]}@hivespace.local",
            $"imp_{source}_{hash[..16]}");
    }

    private static string Normalize(string value)
    {
        var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "unknown" : normalized;
    }
}

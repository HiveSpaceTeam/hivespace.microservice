using System.Globalization;
using System.Text;

namespace HiveSpace.CatalogService.Domain.CatalogImports;

public static class CatalogImportSimilarity
{
    public static bool IsSimilarName(string existing, string incoming)
    {
        var normalizedExisting = Normalize(existing);
        var normalizedIncoming = Normalize(incoming);

        if (normalizedExisting == normalizedIncoming)
            return true;

        if (normalizedExisting.Length >= 12
            && normalizedIncoming.Length >= 12
            && (normalizedExisting.Contains(normalizedIncoming, StringComparison.Ordinal)
                || normalizedIncoming.Contains(normalizedExisting, StringComparison.Ordinal)))
            return true;

        var existingTokens = normalizedExisting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var incomingTokens = normalizedIncoming.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (existingTokens.Length == 0 || incomingTokens.Length == 0)
            return false;

        var overlap = existingTokens.ToHashSet();
        overlap.IntersectWith(incomingTokens);
        return (double)overlap.Count / Math.Min(existingTokens.Length, incomingTokens.Length) >= 0.8;
    }

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var previousWasSpace = false;

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
                previousWasSpace = false;
                continue;
            }

            if (!previousWasSpace)
            {
                builder.Append(' ');
                previousWasSpace = true;
            }
        }

        return builder.ToString().Trim().Normalize(NormalizationForm.FormC);
    }
}

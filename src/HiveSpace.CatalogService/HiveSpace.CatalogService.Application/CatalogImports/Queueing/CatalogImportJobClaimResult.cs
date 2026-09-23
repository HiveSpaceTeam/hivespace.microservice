namespace HiveSpace.CatalogService.Application.CatalogImports.Queueing;

public sealed record CatalogImportJobClaimResult(bool Claimed, string? SkipReason = null)
{
    public static CatalogImportJobClaimResult Success()
        => new(true);

    public static CatalogImportJobClaimResult Skipped(string reason)
        => new(false, reason);
}

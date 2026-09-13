namespace HiveSpace.CatalogService.Infrastructure.CatalogImports;

public interface ICatalogImportServiceTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

namespace HiveSpace.UserService.Infrastructure.IdentityProvisioning;

public interface IIdentityServiceClientTokenProvider
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

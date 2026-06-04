using HiveSpace.IdentityService.Core.Interfaces.Services;

namespace HiveSpace.IdentityService.Api.Endpoints;

internal sealed class RequireBrowserSessionFilter(ITokenCookieService tokenCookieService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        await tokenCookieService.GetRequiredRefreshSessionAsync(context.HttpContext.RequestAborted);
        return await next(context);
    }
}

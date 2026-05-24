using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using HiveSpace.IdentityService.Api.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HiveSpace.IdentityService.Api.Pages.Ciba;
[SecurityHeaders]
[Authorize]
public class AllModel : PageModel
{
    public IEnumerable<BackchannelUserLoginRequest> Logins { get; set; } = default!;

    private readonly IBackchannelAuthenticationInteractionService _backchannelAuthenticationInteraction;

    public AllModel(IBackchannelAuthenticationInteractionService backchannelAuthenticationInteractionService) => _backchannelAuthenticationInteraction = backchannelAuthenticationInteractionService;

    public async Task OnGet() => Logins = await _backchannelAuthenticationInteraction.GetPendingLoginRequestsForCurrentUserAsync();
}

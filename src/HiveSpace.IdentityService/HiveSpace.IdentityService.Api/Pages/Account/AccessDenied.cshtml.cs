using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HiveSpace.IdentityService.Api.Pages.Account;
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class AccessDeniedModel : PageModel{
    public void OnGet()
    {
    }
}

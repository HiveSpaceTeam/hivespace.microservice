using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HiveSpace.UserService.Api.Pages.Account;
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class AccessDeniedModel : PageModel{
    public void OnGet()
    {
    }
}

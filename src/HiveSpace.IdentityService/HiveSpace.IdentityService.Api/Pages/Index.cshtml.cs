using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HiveSpace.IdentityService.Api.Pages;

[AllowAnonymous]
public class Index : PageModel
{
    public IActionResult OnGet()
    {
        return Redirect("/Account/Login");
    }
}

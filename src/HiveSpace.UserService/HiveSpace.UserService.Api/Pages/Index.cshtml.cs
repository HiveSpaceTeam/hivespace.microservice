using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HiveSpace.UserService.Api.Pages;

[AllowAnonymous]
public class Index : PageModel
{
    public IActionResult OnGet()
    {
        // Redirect to login page
        return RedirectToPage("/Account/Login/Index");
    }
}

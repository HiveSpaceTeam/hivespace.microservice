using Duende.IdentityServer.Models;

namespace HiveSpace.IdentityService.Api.Pages.Home.Error;
public class ViewModel
{
    public ViewModel()
    {
    }

    public ViewModel(string error) => Error = new ErrorMessage { Error = error };

    public ErrorMessage? Error { get; set; }
}

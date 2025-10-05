using System.ComponentModel.DataAnnotations;

namespace HiveSpace.UserService.Api.Pages.Account.Register;

public class InputModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email { get; set; }
    
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string? FullName { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
    
    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
    public string? ConfirmPassword { get; set; }
    
    public string? ReturnUrl { get; set; }
    public string? Button { get; set; }
}
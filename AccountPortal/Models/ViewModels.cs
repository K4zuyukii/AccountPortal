using System.ComponentModel.DataAnnotations;

namespace AccountPortal.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}

public class SignupViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Please enter your full name.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Please enter your phone number.")]
    [RegularExpression(@"^\+?[0-9\s\-]{7,15}$", ErrorMessage = "Please enter a valid phone number.")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please enter a password.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Please re-enter your password.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";
}

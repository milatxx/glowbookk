using System.ComponentModel.DataAnnotations;

namespace GlowBook.Web.Models.Account;

public class RegisterViewModel
{
    [Required, EmailAddress]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Naam")]
    public string DisplayName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Paswoord")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Wachtwoorden komen niet overeen")]
    [Display(Name = "Herhaal paswoord")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

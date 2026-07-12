using System.ComponentModel.DataAnnotations;

namespace GlowBook.Web.Models.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Val_Required"), EmailAddress(ErrorMessage = "Val_Email")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_Required")]
    [Display(Name = "Naam")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_Required"), DataType(DataType.Password)]
    [Display(Name = "Wachtwoord")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_Required"), DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Val_PasswordsMismatch")]
    [Display(Name = "Herhaal wachtwoord")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
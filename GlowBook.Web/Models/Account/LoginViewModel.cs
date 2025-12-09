using System.ComponentModel.DataAnnotations;

namespace GlowBook.Web.Models.Account;

public class LoginViewModel
{
    [Required, EmailAddress]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Wachtwoord")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Onthoud mij")]
    public bool RememberMe { get; set; }
}

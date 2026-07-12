using System.ComponentModel.DataAnnotations;

namespace GlowBook.Web.Models;

public class CustomerEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Val_Required"), StringLength(100, ErrorMessage = "Val_StringLength")]
    [Display(Name = "Naam")]
    public string Name { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Val_Phone")]
    [Display(Name = "Telefoon")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Val_Email")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [StringLength(500, ErrorMessage = "Val_StringLength")]
    [Display(Name = "Notities")]
    public string? Notes { get; set; }
}
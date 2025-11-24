using System.ComponentModel.DataAnnotations;

namespace GlowBook.Web.Models;

public class CustomerEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Naam")]
    public string Name { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Telefoon")]
    public string? Phone { get; set; }

    [EmailAddress]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [StringLength(500)]
    [Display(Name = "Notities")]
    public string? Notes { get; set; }
}

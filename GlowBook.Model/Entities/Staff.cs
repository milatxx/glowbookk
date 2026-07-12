using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Staff : BaseEntity
{
    [Required(ErrorMessage = "Val_Required"), StringLength(100, ErrorMessage = "Val_StringLength")]
    [Display(Name = "Naam")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Val_StringLength")]
    public string? RoleName { get; set; }

    [EmailAddress(ErrorMessage = "Val_Email")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Customer : BaseEntity
{
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

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
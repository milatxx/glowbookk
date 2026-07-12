using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Service : BaseEntity
{
    [Required(ErrorMessage = "Val_Required"), StringLength(100, ErrorMessage = "Val_StringLength")]
    [Display(Name = "Naam")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 600, ErrorMessage = "Val_Range")]
    [Display(Name = "Duur (min)")]
    public int DurationMinutes { get; set; }

    [Range(0, 10000, ErrorMessage = "Val_Range")]
    [Display(Name = "Prijs (€)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Val_Required"), StringLength(50, ErrorMessage = "Val_StringLength")]
    [Display(Name = "Categorie")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Val_StringLength")]
    [Display(Name = "Omschrijving")]
    public string? Description { get; set; }

    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}
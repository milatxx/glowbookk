using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Service : BaseEntity
{
    [Required, StringLength(100)]
    [Display(Name = "Naam")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 600)]
    [Display(Name = "Duur (min)")]
    public int DurationMinutes { get; set; }

    [Range(0, 10000)]
    [Display(Name = "Prijs (€)")]
    public decimal Price { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Categorie")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Omschrijving")]
    public string? Description { get; set; }

    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}

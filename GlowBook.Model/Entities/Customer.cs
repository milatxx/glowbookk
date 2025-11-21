using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Customer : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

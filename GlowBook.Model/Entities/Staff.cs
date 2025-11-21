using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Staff : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? RoleName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

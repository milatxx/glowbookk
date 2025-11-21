using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Service : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 600)]
    public int DurationMinutes { get; set; }

    [Range(0, 10000)]
    public decimal Price { get; set; }

    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}

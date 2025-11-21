using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Appointment : BaseEntity
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int StaffId { get; set; }

    [Required]
    public DateTime Start { get; set; }

    [Required]
    public DateTime End { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Ingepland";

    public Customer Customer { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}

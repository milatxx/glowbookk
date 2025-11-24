using System.ComponentModel.DataAnnotations;

namespace GlowBook.Model.Entities;

public class Appointment : BaseEntity
{
    [Required]
    [Display(Name = "Klant")]
    public int CustomerId { get; set; }

    [Display(Name = "Medewerker")]
    [Range(1, int.MaxValue, ErrorMessage = "Medewerker is verplicht")]
    public int StaffId { get; set; }

    [Required]
    [Display(Name = "Start")]
    public DateTime Start { get; set; }

    [Required]
    [Display(Name = "Einde")]
    public DateTime End { get; set; }

    [StringLength(50)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Ingepland";

    public Customer Customer { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
    public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
}

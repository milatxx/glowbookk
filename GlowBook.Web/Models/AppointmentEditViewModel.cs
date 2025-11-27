using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlowBook.Web.Models
{
    public class AppointmentEditViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Klant is verplicht")]
        [Display(Name = "Klant")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Medewerker is verplicht")]
        [Display(Name = "Medewerker")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Dienst is verplicht")]
        [Display(Name = "Dienst")]
        public int ServiceId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Starttijd")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Range(15, 480)]
        [Display(Name = "Duur (min)")]
        public int DurationMinutes { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Ingepland";

        // Dropdowns
        public SelectList? Customers { get; set; }
        public SelectList? Staff { get; set; }
        public SelectList? Services { get; set; }
    }
}

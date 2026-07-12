using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlowBook.Web.Models
{
    public class AppointmentEditViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Val_Required")]
        [Display(Name = "Klant")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Val_Required")]
        [Display(Name = "Medewerker")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Val_Required")]
        [Display(Name = "Dienst")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Val_Required")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Val_Required")]
        [DataType(DataType.Time)]
        [Display(Name = "Starttijd")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Val_Required")]
        [Range(15, 480, ErrorMessage = "Val_Range")]
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
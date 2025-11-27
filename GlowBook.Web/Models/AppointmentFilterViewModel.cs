using System.ComponentModel.DataAnnotations;
using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlowBook.Web.Models
{
    public class AppointmentFilterViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Vanaf")]
        public DateTime? From { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Tot")]
        public DateTime? To { get; set; }

        [Display(Name = "Klant")]
        public int? CustomerId { get; set; }

        [Display(Name = "Medewerker")]
        public int? StaffId { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        // Dropdowns
        public SelectList? Customers { get; set; }
        public SelectList? Staff { get; set; }
        public SelectList? Statuses { get; set; }

        public List<Appointment> Items { get; set; } = new();
    }
}

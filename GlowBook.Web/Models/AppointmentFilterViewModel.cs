using System.ComponentModel.DataAnnotations;

namespace GlowBook.Web.Models;

public class AppointmentFilterViewModel
{
    [Display(Name = "Datum vanaf")]
    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [Display(Name = "Datum tot")]
    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    [Display(Name = "Medewerker")]
    public int? StaffId { get; set; }

    public string? SortOrder { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

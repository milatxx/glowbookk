using SQLite;

namespace GlowBook.Mobile.Models;

public class LocalAppointment
{
    [PrimaryKey]              // Id van de server
    public int Id { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

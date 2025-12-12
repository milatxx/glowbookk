using SQLite;

namespace GlowBook.Mobile.Models;

public class LocalAppointment
{
    [PrimaryKey, AutoIncrement] // Lokale Id in SQLite
    public int Id { get; set; }

    public int ServerId { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
